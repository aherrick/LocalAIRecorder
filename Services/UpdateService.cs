using System.Xml.Linq;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace LocalAIRecorder.Services;

public static class UpdateService
{
    public static async Task CheckForUpdatesAsync()
    {
        try
        {
            using var client = new HttpClient();
            var url =
                $"https://localairecorder.z19.web.core.windows.net/ios/manifest.plist?t={DateTime.UtcNow.Ticks}";
            var plistXml = await client.GetStringAsync(url);
            var doc = XDocument.Parse(plistXml);

            var versionElement = doc.Descendants("key")
                .FirstOrDefault(k => k.Value == "bundle-version")
                ?.ElementsAfterSelf("string")
                .FirstOrDefault();

            Version? remoteVersion = null;
            if (
                versionElement != null
                && Version.TryParse(versionElement.Value, out var parsedRemote)
            )
            {
                remoteVersion = parsedRemote;
                if (remoteVersion > AppInfo.Current.Version)
                {
                    await Launcher.OpenAsync("https://localairecorder.z19.web.core.windows.net");
                }
            }

            // Debug toast: show current vs remote (or unknown)
            var current = AppInfo.Current.VersionString;
            var remoteText = remoteVersion?.ToString() ?? "(none)";
            var toastMessage = $"Version: current {current} / remote {remoteText}";

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var toast = Toast.Make(toastMessage, ToastDuration.Short);
                await toast.Show();
            });
        }
        catch
        {
            // Silent fail; optional failure toast could go here if needed
        }
    }
}