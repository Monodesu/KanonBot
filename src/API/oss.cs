
using Aliyun.OSS.Common;
using Aliyun.OSS;
using Serilog;


namespace KanonBot.API;
public class Ali
{
    private static Config.OSS config = Config.inner!.oss!;
    public static string? PutFile(string key, Stream data)
    {
        var client = new OssClient(config.endPoint, config.accessKeyId!, config.accessKeySecret!);
        try
        {                
            var res = client.PutObject(config.bucketName!, key, data);
            return config.url + key;
        }
        catch (OssException ex)
        {
            Log.Error("oss上传文件失败: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
        }
        catch (Exception ex)
        {
            Log.Error("oss上传文件失败: {0}", ex.Message);
        }
        return null;
    }

    // ext 为文件扩展名，不带点
    public static string? PutFile(Stream data, string ext, bool temp = true)
    {
        if (temp)
            return PutFile($"temp-{Guid.NewGuid().ToString()[0..6]}.{ext}", data);
        else
            return PutFile($"{Guid.NewGuid().ToString()[0..6]}.{ext}", data);
    }

    public static void ListAllBuckets()
    {
        var client = new OssClient(config.endPoint, config.accessKeyId!, config.accessKeySecret!);
        var buckets = client.ListBuckets();

        foreach (var bucket in buckets)
        {
            Log.Information(bucket.Name + ", " + bucket.Location + ", " + bucket.Owner);
        }
    }
}