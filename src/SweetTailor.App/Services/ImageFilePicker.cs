namespace SweetTailor.App.Services;

public static class ImageFilePicker
{
    public static async Task<string?> PickAndSaveImageAsync(string title)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = title,
            FileTypes = FilePickerFileType.Images
        });

        if (result is null)
        {
            return null;
        }

        var extension = Path.GetExtension(result.FileName);
        var destination = Path.Combine(
            FileSystem.AppDataDirectory,
            $"{Guid.NewGuid():N}{extension}");

        await using var source = await result.OpenReadAsync();
        await using var target = File.Create(destination);
        await source.CopyToAsync(target);

        return destination;
    }
}
