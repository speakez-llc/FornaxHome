module.exports = {
  plugins: [
    // Stage 1: Pre-processing
    require('tailwindcss'),
    
    // Stage 2: Core processing with extensions
    require('tailwindcss/nesting'),
    
    // Stage 3: Vendor prefixing and compatibility
    require('autoprefixer'),
    
    // Stage 4: Optimization (only in production)
    ...(process.env.NODE_ENV === 'production' 
      ? [require('cssnano')({ preset: 'default' })]
      : [])
  ],
}