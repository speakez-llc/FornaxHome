# FornaxHome

This is the SpeakEZ base site built with Fornax with a connection to a back end using Suave.

## Technologies

- **Fornax** - F# static site generator
- **Suave** - Lightweight web server library for F#
- **Tailwind CSS** - Utility-first CSS framework
- **DaisyUI** - Component library for Tailwind CSS

## Project Structure

- `src/generator/` - Fornax site generator code and content
- `src/server/` - Suave web server implementation
- `src/generator/style/` - CSS styling with Tailwind and DaisyUI

## Getting Started

1. Install dependencies
```sh
dotnet restore
```

2. Run the development server
```sh
cd src/server
dotnet run
```

3. Run the Fornax generator (in separate terminal)
```sh
cd src/generator
fornax watch
```

## Features

- Modern responsive design using Tailwind CSS and DaisyUI components
- Dark/Light theme switching with `corporate` and `business` themes
- Live reload during development
- Static site generation for production deployment

## License

This project is licensed under the MIT License.