namespace Nexytrus
{
    public record FileInfos
    {
        public string FilePath;
        public string OriginalFilePath;
        public string Hash;
        public long Size;

        public FileInfos(string filePath, string originalFilePath, string hash, long size)
        {
            FilePath = filePath;
            OriginalFilePath = originalFilePath;
            Hash = hash;
            Size = size;
        }
    }
}
