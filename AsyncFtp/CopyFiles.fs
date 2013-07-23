namespace AsyncFtp

open System.IO
open System.Net
open System.Text
open Microsoft.FSharp.Control.WebExtensions
open Microsoft.FSharp.Control.StreamReaderExtensions

type public CopyFiles() =

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
        let computations = [for i in 1..n -> x.UploadFileAsync serverIpAddress directory (sprintf "%s%d.txt" fileNameBase i) fileContents]
        Async.Parallel computations |> Async.RunSynchronously

    member private x.UploadFileAsync serverIpAddress directory fileName fileContents =
        async {
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
            return! reader.AsyncReadToEnd()
        }
