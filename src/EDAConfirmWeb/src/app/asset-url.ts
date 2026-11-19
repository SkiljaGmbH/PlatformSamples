// Resolves a file in the deployed `assets/` folder to an absolute URL.
// This module's URL points at the bundle, which always sits at the deploy root next to
// `assets/`, so this is correct at any hosting sub-path. A root-relative path would instead
// always resolve against the server root, and a document-relative one against the current
// document URL, since HttpClient ignores <base href>.
//
// `import.meta.url` is read into a variable first: passing it directly to `new URL()` with a
// non-static first argument makes Vite's import-glob plugin treat the call as a glob import
// and fail the dev server build.
const moduleUrl = import.meta.url;
const deployRoot = new URL('.', moduleUrl);

export function assetUrl(path: string): string {
  return new URL(`assets/${path}`, deployRoot).toString();
}
