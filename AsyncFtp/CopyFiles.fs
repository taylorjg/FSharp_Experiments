namespace AsyncFtp

open System
open System.IO
open System.Net
open System.Text
open System.Collections.Concurrent
open System.Collections.Generic
open Microsoft.FSharp.Control.WebExtensions
open Microsoft.FSharp.Control.StreamReaderExtensions

type private AsyncStream(stream : Stream) =

    static let defaultBufferSize = 4096

    member public x.ReadToEnd() =
        async {
                let overallBuffer = ref (Array.zeroCreate<byte> 0)

                let buffer = Array.zeroCreate<byte> defaultBufferSize

                let! temp = stream.AsyncRead(buffer)
                let cb = ref temp

                while !cb > 0 do

                    let oldOverallBufferLength = (!overallBuffer).Length
                    let newOverallBufferLength = oldOverallBufferLength + !cb
                    let newOverallBuffer = Array.zeroCreate<byte> newOverallBufferLength
                    do System.Buffer.BlockCopy (!overallBuffer, 0, newOverallBuffer, 0, oldOverallBufferLength)
                    do System.Buffer.BlockCopy (buffer, 0, newOverallBuffer, oldOverallBufferLength, !cb)
                    overallBuffer := newOverallBuffer

                    let! temp = stream.AsyncRead(buffer)
                    cb := temp

                return !overallBuffer
        }            

type NotificationEventArgs(message:string) =
    inherit EventArgs()
    member this.Message = message

type public NotificationEventDelegate = delegate of obj * NotificationEventArgs -> unit

type public CopyFiles(ftpUserName : string, ftpPassword : string) =

    let credentials = new NetworkCredential(ftpUserName, ftpPassword)
    let MyConnectionGroupName = "FtpWebRequest-JT"

    let notificationEvent = new Event<NotificationEventDelegate, NotificationEventArgs>()

    member private x.Notify(message) =
        let mtid = System.Threading.Thread.CurrentThread.ManagedThreadId
        notificationEvent.Trigger(x, new NotificationEventArgs(sprintf "[%d] %s" mtid message))

    [<CLIEvent>]
    member public x.Notification = notificationEvent.Publish

    member public x.UploadFile serverIpAddress directory fileName (fileContents : byte[]) =
        let uri = sprintf "ftp://%s/%s/%s" serverIpAddress directory fileName
        let request = WebRequest.Create(uri, Method = WebRequestMethods.Ftp.UploadFile, Credentials = credentials) :?> FtpWebRequest
        request.KeepAlive <- true
        request.UseBinary <- true
        begin
            use requestStream = request.GetRequestStream()
            use stream = new MemoryStream(fileContents)
            stream.CopyTo(requestStream)
        end
        use response = request.GetResponse()
        ()

    member public x.UploadFilesInParallel n serverIpAddress directory fileNameBase fileContents =
        x.Notify "Beginning UploadFilesInParallel"
        let computations = [for i in 1..n -> x.UploadFileAsync serverIpAddress directory (sprintf "%s%d.txt" fileNameBase i) fileContents]
        Async.Parallel computations |> Async.RunSynchronously |> ignore
        x.Notify "Ending UploadFilesInParallel"
        ()

    // TODO: add a bool flag to NotificationEventArgs to differentiate between info and error messages ?
    // - or use a separate event for errors ?
    // - or raise an exception ? or several exceptions ?
    // - also, pass in a bunch of counters - downloaded, uploaded, skipped, etc.

    member public x.CopyFilesInParallel (dict:ConcurrentDictionary<string, byte[]>) (listing:IList<string>) srcIp srcDir dstIp dstDir =
        x.Notify "Beginning CopyFilesInParallel"
        let computations = [
            for fileName in dict.Keys -> async {
                try
                    if not (listing.Contains(fileName)) then
                        let fileContents = ref dict.[fileName]
                        if !fileContents = null then
                            let! temp = x.DownloadFileAsync srcIp srcDir fileName
                            dict.[fileName] <- temp
                            fileContents := temp
                        do! x.UploadFileAsync dstIp dstDir fileName !fileContents
                    else
                        x.Notify (sprintf @"Skipping ""%s""" fileName)
                with
                    err -> x.Notify (sprintf @"Exception in CopyFiles for file ""%s"": %s" fileName err.Message)
            }
        ]
        Async.Parallel computations |> Async.RunSynchronously |> ignore
        x.Notify "Ending CopyFilesInParallel"
        ()

    member private x.UploadFileAsync serverIpAddress directory fileName fileContents =
        async {
            x.Notify (sprintf "Beginning UploadFileAsync (%s)" fileName)
            let uri = sprintf "ftp://%s/%s/%s" serverIpAddress directory fileName
            let request = WebRequest.Create(uri, Method = WebRequestMethods.Ftp.UploadFile, Credentials = credentials) :?> FtpWebRequest
            request.KeepAlive <- true
            request.UseBinary <- true
            request.ConnectionGroupName <- MyConnectionGroupName
            do! async {
                use requestStream = request.GetRequestStream()
                do! requestStream.AsyncWrite(fileContents)
            }
            use! response = request.AsyncGetResponse()
            x.Notify (sprintf "Ending UploadFileAsync (%s)" fileName)
            ()
        }

    member private x.DownloadFileAsync srcIp srcDir fileName = 
        async {
            x.Notify (sprintf "Beginning DownloadFile (%s)" fileName)
            let uri = sprintf "ftp://%s/%s/%s" srcIp srcDir fileName
            let request = WebRequest.Create(uri, Method = WebRequestMethods.Ftp.DownloadFile, Credentials = credentials) :?> FtpWebRequest
            request.KeepAlive <- true
            request.UseBinary <- true
            request.ConnectionGroupName <- MyConnectionGroupName
            use! response = request.AsyncGetResponse()
            use responseStream = response.GetResponseStream()
            let asyncStream = new AsyncStream(responseStream)
            let! result = asyncStream.ReadToEnd()
            x.Notify (sprintf "Ending DownloadFile (%s)" fileName)
            return result
        }
