namespace AutoWise.Media.Application.Dtos;

public record MediaDownloadResult(Stream Content, string ContentType, string FileName);
