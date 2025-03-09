const fs = require('fs');
const path = require('path');
const postcss = require('postcss');
const tailwindcssPostcss = require('@tailwindcss/postcss');
const autoprefixer = require('autoprefixer');
const cssnano = require('cssnano');

// Define paths
const inputPath = 'style/style.css';
const outputPath = '_public/style/style.css';
const configPath = 'tailwind.config.js';

// Ensure output directory exists
const outputDir = path.dirname(outputPath);
if (!fs.existsSync(outputDir)) {
  fs.mkdirSync(outputDir, { recursive: true });
}

// Read the input CSS file
const css = fs.readFileSync(inputPath, 'utf8');

// Process the CSS
postcss([
  tailwindcssPostcss(configPath),
  autoprefixer,
  cssnano({ preset: 'default' })
])
  .process(css, { from: inputPath, to: outputPath })
  .then(result => {
    // Write the processed CSS to the output file
    fs.writeFileSync(outputPath, result.css);
    console.log('CSS built successfully!');
  })
  .catch(error => {
    console.error('Error processing CSS:', error);
  });