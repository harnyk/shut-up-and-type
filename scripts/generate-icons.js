const sharp = require('sharp');
const fs = require('fs-extra');
const path = require('path');
const pngToIco = require('png-to-ico');

const SIZES = [16, 20, 24, 32, 40, 48, 64, 96, 128, 256, 512];

async function generateIcons() {
  console.log('🎨 ShutUpAndType Icon Generation');
  console.log('=================================');

  // Ensure directories exist
  await fs.ensureDir('assets/icons/generated');
  await fs.ensureDir('assets/icons/ico');

  const sources = [
    {
      svg: 'assets/icons/source/microphone.svg',
      name: 'microphone',
      description: 'normal microphone'
    },
    {
      svg: 'assets/icons/source/microphone-recording.svg',
      name: 'microphone-recording',
      description: 'recording microphone'
    }
  ];

  // Generate PNGs for all sources
  for (const source of sources) {
    if (await fs.pathExists(source.svg)) {
      console.log(`\nℹ️  Generating ${source.description} PNGs...`);

      for (const size of SIZES) {
        const outputFile = `assets/icons/generated/${source.name}-${size}x${size}.png`;
        console.log(`  Generating ${size}x${size}...`);

        await sharp(source.svg)
          .resize(size, size)
          .flatten({ background: { r: 255, g: 255, b: 255 } }) // White background
          .png()
          .toFile(outputFile);
      }
    } else {
      console.log(`⚠️  ${source.svg} not found, skipping ${source.description}`);
    }
  }

  console.log('\n✅ PNG generation complete!');

  // Generate ICO files using png-to-ico npm package
  try {
    console.log('\nℹ️  Generating ICO files...');

    // Main application icon
    const mainIconSizes = [16, 32, 48, 256];
    const mainIconPaths = [];
    for (const size of mainIconSizes) {
      const pngPath = `assets/icons/generated/microphone-${size}x${size}.png`;
      if (await fs.pathExists(pngPath)) {
        mainIconPaths.push(pngPath);
      }
    }

    if (mainIconPaths.length > 0) {
      const mainIcoBuffer = await pngToIco(mainIconPaths);
      await fs.writeFile('assets/icons/ico/microphone.ico', mainIcoBuffer);
      console.log('  Generated microphone.ico');
    }

    // System tray icon
    const trayIconSizes = [16, 20, 24, 32];
    const trayIconPaths = [];
    for (const size of trayIconSizes) {
      const pngPath = `assets/icons/generated/microphone-${size}x${size}.png`;
      if (await fs.pathExists(pngPath)) {
        trayIconPaths.push(pngPath);
      }
    }

    if (trayIconPaths.length > 0) {
      const trayIcoBuffer = await pngToIco(trayIconPaths);
      await fs.writeFile('assets/icons/ico/microphone-tray.ico', trayIcoBuffer);
      console.log('  Generated microphone-tray.ico');
    }

    // Recording system tray icon
    if (await fs.pathExists('assets/icons/generated/microphone-recording-16x16.png')) {
      const recordingTrayPaths = [];
      for (const size of trayIconSizes) {
        const pngPath = `assets/icons/generated/microphone-recording-${size}x${size}.png`;
        if (await fs.pathExists(pngPath)) {
          recordingTrayPaths.push(pngPath);
        }
      }

      if (recordingTrayPaths.length > 0) {
        const recordingIcoBuffer = await pngToIco(recordingTrayPaths);
        await fs.writeFile('assets/icons/ico/microphone-recording-tray.ico', recordingIcoBuffer);
        console.log('  Generated microphone-recording-tray.ico');
      }
    }

    // Note: No need to copy to root - .csproj and installer reference assets/icons/ico/ directly

    console.log('\n✅ ICO generation complete!');

  } catch (error) {
    console.log('\n⚠️  ICO generation failed:', error.message);
    throw error;
  }

  console.log('\n🎉 Icon generation finished!');

  // Show generated files
  console.log('\nGenerated files:');
  console.log('  📁 assets/icons/generated/ - PNG files');

  if (await fs.pathExists('assets/icons/ico/microphone.ico')) {
    console.log('  📁 assets/icons/ico/ - ICO files');
    console.log('  📄 microphone.ico - Main application icon');
    console.log('  📄 microphone-tray.ico - Normal tray icon');

    if (await fs.pathExists('assets/icons/ico/microphone-recording-tray.ico')) {
      console.log('  📄 microphone-recording-tray.ico - Recording tray icon');
    }
  }
}

// Run the generation
generateIcons().catch(console.error);