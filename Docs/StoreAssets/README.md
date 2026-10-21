# Microsoft Store artwork

Generated from `LocalKnights.WindowsDesktop/Resources/Icon/logo.png` and
`logo-bg.png` using `Scripts/create-store-assets.ps1`. When the script runs, it
downloads Space Grotesk Bold over HTTPS from Google Fonts; no font file is stored in
the repository. The font is distributed under the
[SIL Open Font License](https://openfontlicense.org/).

| File | Dimensions | Partner Center slot |
| --- | ---: | --- |
| `StoreLogo-Poster-1440x2160.png` | 1440 × 2160 | Store logos — 9:16 poster art |
| `StoreLogo-BoxArt-2160x2160.png` | 2160 × 2160 | Store logos — 1:1 box art |
| `StoreDisplay-AppTile-300x300.png` | 300 × 300 | Store display — app tile icon |
| `StoreDisplay-AppTile-150x150.png` | 150 × 150 | Store display — 1:1 logo |
| `StoreDisplay-AppTile-71x71.png` | 71 × 71 | Store display — 1:1 logo |
| `WindowsXbox-SuperHero-3840x2160.png` | 3840 × 2160 | Windows and Xbox — 16:9 super hero art |
| `Xbox-BrandedKeyArt-584x800.png` | 584 × 800 | Xbox — branded key art |
| `Xbox-TitledHero-1920x1080.png` | 1920 × 1080 | Xbox — titled hero art |
| `Xbox-FeaturedPromotionalSquare-1080x1080.png` | 1080 × 1080 | Xbox — featured promotional square art |

The super hero and featured promotional square intentionally do not contain the
product title. The featured square centers the logo without extra accent lines. The
poster, box art, branded key art, and titled hero display “Local Knights” in Space
Grotesk, with “Local” in white and “Knights” in orange. Images are optional or
Xbox-specific unless noted by Partner Center; upload only the assets for the device
families and listing sections you intend to support.

The script requires an internet connection to download the font. Override its
download location with `-FontUri` if needed.

Regenerate the files from the repository root with:

```powershell
.\Scripts\create-store-assets.ps1
```
