import daisyui from "daisyui"

const ACCENT_COLOR = '#ED5B00';

export default {
  content: [
    "./_public/**/*.html",
    "./style/**/*.css",
  ],
  theme: {
    extend: {
      colors: {
        'accent': ACCENT_COLOR
      },
      animation: {
        'fadeIn': 'fadeIn 500ms ease-in-out',
        'fadeOut': 'fadeOut 500ms ease-in-out',
        'pulse-ring': 'pulse-ring 1s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'pulse-button': 'pulse-button 1s cubic-bezier(0.4, 0, 0.6, 1) infinite'
      },
      keyframes: {
        'fadeIn': {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        'fadeOut': {
          '0%': { opacity: '1' },
          '100%': { opacity: '0' },
        },
        'pulse-ring': {
          '0%, 100%': {
            borderColor: 'theme("colors.accent")',
            boxShadow: '0 0 0 1px theme("colors.accent")'
          },
          '50%': {
            borderColor: 'theme("colors.accent-light")',
            boxShadow: '0 0 0 2px theme("colors.accent-light")'
          },
        },
        'pulse-button': {
          '0%, 100%': {
            outline: '2px solid theme("colors.accent")',
            outlineOffset: '1px'
          },
          '50%': {
            outline: '3px solid theme("colors.accent-light")',
            outlineOffset: '2px'
          },
          shadowRoll: {
              '0%, 100%': { boxShadow: '1px -1px 10px' },
              '12.5%': { boxShadow: '1px 1px 10px' },
              '25%': { boxShadow: '-1px 1px 10px' },
              '37.5%': { boxShadow: '-1px -1px 10px' },
              '50%': { boxShadow: '1px -1px 10px' },
              '62.5%': { boxShadow: '1px 1px 10px' },
              '75%': { boxShadow: '-1px 1px 10px' },
              '87.5%': { boxShadow: '-1px -1px 10px' }
          }
        }
      },
      hljs: {
        theme: 'pojoaque',
        cssVar: true,
        extend: {
        }
      },
      typography: {
        DEFAULT: {
          css: {
            maxWidth: '65ch',
            color: 'var(--tw-prose-body)',
            a: {
              color: 'var(--tw-prose-links)',
              textDecoration: 'underline',
              fontWeight: '500',
            },
            pre: {
              padding: '1.25em 1.5em',
              backgroundColor: 'var(--tw-prose-pre-bg)',
              border: '1px solid var(--tw-prose-pre-border)',
              borderRadius: '0.375rem',
            },
            code: {
              color: 'var(--tw-prose-code)',
              borderRadius: '0.25rem',
            },
            'code::before': {
              content: 'none',
            },
            'code::after': {
              content: 'none',
            },
          }
        }
      },
      transitionProperty: {
        'opacity': 'opacity',
        'transform': 'transform'
      },
      transitionDuration: {
        '500': '500ms'
      },
      transitionTimingFunction: {
        'ease-in-out': 'ease-in-out'
      }
    },
  },
  plugins: [
    require("daisyui"),
    require("tailwindcss-markdown"),
    require("@tailwindcss/typography"),
    require("tailwind-highlightjs"),
    require("@tailwindcss/container-queries"),
  ],
  daisyui: {
    themes: [
      "dark",
      "cupcake",
      "bumblebee",
      "emerald",
      "corporate",
      "synthwave",
      "retro",
      "cyberpunk",
      "valentine",
      "halloween",
      "garden",
      "forest",
      "aqua",
      "lofi",
      "pastel",
      "fantasy",
      "wireframe",
      "black",
      "luxury",
      "dracula",
      "cmyk",
      "autumn",
      "acid",
      "lemonade",
      "night",
      "coffee",
      "winter",
      "dim",
      "nord",
      "sunset",
      "business",
      "corporate"
    ],
    base: true,
    styled: true,
    utils: true,
    prefix: "",
    logs: true,
    themeRoot: ":root",
  },
  daisyui: {
    themes: true, // false: only light + dark | true: all themes | array: specific themes like this ["light", "dark", "cupcake"]
    darkTheme: "dark", // name of one of the included themes for dark mode
    base: true, // applies background color and foreground color for root element by default
    styled: true, // include daisyUI colors and design decisions for all components
    utils: true, // adds responsive and modifier utility classes
    prefix: "", // prefix for daisyUI classnames (components, modifiers and responsive class names. Not colors)
    logs: true, // Shows info about daisyUI version and used config in the console when building your CSS
    themeRoot: ":root", // The element that receives theme color CSS variables
  },
}