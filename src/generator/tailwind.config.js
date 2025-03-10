module.exports = {
  content: [
    "./_public/**/*.html",
    "./style/**/*.css",
  ],
  safelist: [{
    pattern: /hljs+/,
  }],
  theme: {
    extend: {
      colors: {
        'accent-light': '#d2be68',
        'accent': '#ED5B00',
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
          }
        }
      },
      hljs: {
        theme: 'an-old-hope',
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
      "light",
      "dark",
    ],
    darkTheme: "dark",
    base: true,
    styled: true,
    utils: true,
    prefix: "",
    logs: true,
    themeRoot: ":root",
  },
  // Enable dark mode
  darkMode: 'class',
}