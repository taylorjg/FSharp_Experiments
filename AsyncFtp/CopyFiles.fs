namespace AsyncFtp

open System
open System.IO
open System.Net
open System.Text
open Microsoft.FSharp.Control.WebExtensions
open Microsoft.FSharp.Control.StreamReaderExtensions

type NotificationEventArgs(message:string) =
    inherit EventArgs()
    member this.Message = message

type public NotificationEventDelegate = delegate of obj * NotificationEventArgs -> unit

type public CopyFiles() =

    let notificationEvent = new Event<NotificationEventDelegate, NotificationEventArgs>()

    member private x.Notify(message) = notificationEvent.Trigger(x, new NotificationEventArgs(message))

    [<CLIEvent>]
    member public x.Notification = notificationEvent.Publish

    member public x.UploadFile serverIpAddress directory fileName (fileContents : byte[]) =
        let uri = sprintf "ftp://%s/%s/%s" serverIpAddress directory fileName
        let nc = new NetworkCredential("env6ftp", "w1nd0w5.")
        let request = WebRequest.Create(uri, Method = WebRequestMethods.Ftp.UploadFile, Credentials = nc) :?> FtpWebRequest
        request.KeepAlive <- false
        begin
            use requestStream = request.GetRequestStream()
            use stream = new MemoryStream(fileContents)
            stream.CopyTo(requestStream)
        end
        use response = request.GetResponse()
        use responseStream = response.GetResponseStream()
        use reader = new StreamReader(responseStream)
        reader.ReadToEnd()

    member public x.UploadFiles n serverIpAddress directory fileNameBase fileContents =
        x.Notify "Beginning UploadFiles"
        let computations = [for i in 1..n -> x.UploadFileAsync serverIpAddress directory (sprintf "%s%d.txt" fileNameBase i) fileContents]
        let result = Async.Parallel computations |> Async.RunSynchronously
        x.Notify "Ending UploadFiles"
        result

    member private x.UploadFileAsync serverIpAddress directory fileName fileContents =
        async {
            x.Notify (sprintf "Beginning (%s)" fileName)
            let uri = sprintf "ftp://%s/%s/%s" serverIpAddress directory fileName
            let nc = new NetworkCredential("env6ftp", "w1nd0w5.")
            let request = WebRequest.Create(uri, Method = WebRequestMethods.Ftp.UploadFile, Credentials = nc) :?> FtpWebRequest
            request.KeepAlive <- false
            request.UseBinary <- false
            use requestStream = request.GetRequestStream()
            do! requestStream.AsyncWrite(fileContents)
            requestStream.Close()
            use! response = request.AsyncGetResponse()
            use responseStream = response.GetResponseStream()
            use reader = new StreamReader(responseStream)
            x.Notify (sprintf "Ending (%s)" fileName)
            return! reader.AsyncReadToEnd()
        }
