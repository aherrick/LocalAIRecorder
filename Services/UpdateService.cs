using System.Xml.Linq;

namespace LocalAIRecorder.Services;

public static class UpdateService
{
    public static async Task CheckForUpdatesAsync()
    {
#if !DEBUG
        try
        {
            // Dead simple check against the distribution manifest
            using var client = new HttpClient();
            // Cache busting to ensure we get the latest
            // Note: manifest is at /ios/manifest.plist per ios.yml
            var url = $"https://localairecorder.z19.web.core.windows.net/ios/manifest.plist?t={DateTime.UtcNow.Ticks}";
            var plistXml = await client.GetStringAsync(url);
            
            var doc = XDocument.Parse(plistXml);
            
            // Find the <key>bundle-version</key> and get the next <string> element
            var versionElement = doc.Descendants("key")
                .FirstOrDefault(k => k.Value == "bundle-version")
                ?.ElementsAfterSelf("string")
                .FirstOrDefault();

            if (versionElement != null && Version.TryParse(versionElement.Value, out var remoteVersion))
            {
                if (remoteVersion > AppInfo.Current.Version)
                {
                    // Open the install page
                    await Launcher.OpenAsync("https://localairecorder.z19.web.core.windows.net");
                }
            }
        }
        catch
        {
            // Silent fail for background check
        }
#else
        await Task.CompletedTask;
#endif
    }
}
