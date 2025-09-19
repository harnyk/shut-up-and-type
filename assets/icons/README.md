# Icons

This directory contains icon assets for the ShutUpAndType application.

## Structure

- `source/` - Source SVG files (vector graphics)
- `generated/` - Auto-generated icons at various resolutions
- `ico/` - Windows ICO files with multiple resolutions

## Icon Sizes

The following icon sizes are generated:

### Standard Windows Sizes
- **16x16** - Small icon (taskbar, window title)
- **24x24** - Small toolbar icons
- **32x32** - Standard icon (desktop, file explorer)
- **48x48** - Large icon (desktop, high DPI)
- **64x64** - Extra large icon
- **96x96** - High DPI medium icon
- **128x128** - High DPI large icon
- **256x256** - Vista/Windows 7+ large icon
- **512x512** - High resolution for future use

### Application Specific
- **20x20** - Notification area (system tray) icon
- **40x40** - High DPI notification area icon

## CI/CD Process

Icons are automatically generated from the source SVG file using GitHub Actions:

1. **Source Update**: When `source/microphone.svg` is modified
2. **Generation**: CI generates all required resolutions as PNG files
3. **ICO Creation**: Multiple PNGs are combined into Windows ICO files
4. **Optimization**: Icons are optimized for size and quality
5. **Validation**: Generated icons are validated for compliance

## Manual Generation

To manually generate icons locally:

```bash
# Install dependencies
npm install -g svg2png-cli imagemin-cli

# Generate PNGs from SVG
svg2png assets/icons/source/microphone.svg -o assets/icons/generated/ -w 16 -h 16
svg2png assets/icons/source/microphone.svg -o assets/icons/generated/ -w 32 -h 32
# ... (repeat for all sizes)

# Create ICO file
magick convert assets/icons/generated/microphone-*.png assets/icons/ico/microphone.ico
```

## Best Practices

1. **Source Control**: Only commit source SVG files, not generated assets
2. **Vector First**: Always create icons as vector graphics (SVG) for scalability
3. **Pixel Perfect**: Ensure icons look crisp at all target sizes
4. **High Contrast**: Use sufficient contrast for accessibility
5. **Consistent Style**: Maintain visual consistency across all icon sizes