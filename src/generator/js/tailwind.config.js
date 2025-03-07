module.exports = {
                    content: ["./**/*.{html,fsx}"],
                    theme: {
                        extend: {
                            colors: {
                                'accent-light': '#d2be68'
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
                    plugins: [require("daisyui")],
                    daisyui: {
                        themes: [
                            {
                                business: {
                                    ...require("daisyui/src/theming/themes")["business"],
                                    accent: "#ED5B00",
                                },
                            },
                            {
                                nord: {
                                    ...require("daisyui/src/theming/themes")["corporate"],
                                    accent: "#ED5B00",
                                },
                            }
                        ],
                        base: true,
                        styled: true,
                        utils: true,
                        prefix: "",
                        logs: true,
                        themeRoot: ":root",
                    },
                }