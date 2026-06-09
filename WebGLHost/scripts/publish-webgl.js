#!/usr/bin/env node

const { spawnSync } = require("node:child_process");
const fs = require("node:fs");
const path = require("node:path");

const hostRoot = path.resolve(__dirname, "..");
const projectRoot = path.resolve(hostRoot, "..");
const sourceBuild = resolveSourceBuild();
const targetBuild = path.resolve(hostRoot, "game");
const shouldDeploy = !process.argv.includes("--no-deploy");

function getArgValue(name) {
  const index = process.argv.indexOf(name);
  if (index === -1) {
    return null;
  }

  return process.argv[index + 1] || null;
}

function resolveSourceBuild() {
  const sourceArg = getArgValue("--source");
  if (!sourceArg) {
    return path.resolve(projectRoot, "webglbuild");
  }

  if (path.isAbsolute(sourceArg)) {
    return path.resolve(sourceArg);
  }

  const projectRelative = path.resolve(projectRoot, sourceArg);
  if (fs.existsSync(projectRelative)) {
    return projectRelative;
  }

  return path.resolve(process.cwd(), sourceArg);
}

function fail(message) {
  console.error(`publish-webgl: ${message}`);
  process.exit(1);
}

function read(file) {
  return fs.readFileSync(file, "utf8");
}

function write(file, contents) {
  fs.writeFileSync(file, contents);
}

function assertExists(file, label) {
  if (!fs.existsSync(file)) {
    fail(`${label} not found at ${file}`);
  }
}

function detectBuildName(buildDir) {
  const files = fs.readdirSync(buildDir);
  const loader = files.find((file) => file.endsWith(".loader.js"));
  if (!loader) {
    fail(`Unity loader file not found in ${buildDir}`);
  }

  return loader.slice(0, -".loader.js".length);
}

function copyBuildOutput() {
  assertExists(sourceBuild, "Unity WebGL build folder");
  assertExists(path.join(sourceBuild, "index.html"), "Unity WebGL index");
  assertExists(path.join(sourceBuild, "Build"), "Unity WebGL Build folder");

  if (sourceBuild === targetBuild) {
    return;
  }

  fs.rmSync(targetBuild, { recursive: true, force: true });
  fs.cpSync(sourceBuild, targetBuild, { recursive: true });
}

function patchUnityIndex(buildName, buildVersion) {
  const indexPath = path.join(targetBuild, "index.html");
  let html = read(indexPath);

  html = html.replace(
    /<canvas id="unity-canvas"[^>]*><\/canvas>/,
    '<canvas id="unity-canvas" tabindex="-1"></canvas>'
  );

  html = html.replace(
    /^\s*var buildUrl = "Build";\n(?:\s*var buildVersion = "[^"]+";\n)?\s*var loaderUrl = [^\n]+;$/m,
    `      var buildUrl = "Build";\n      var buildVersion = "${buildVersion}";\n      var loaderUrl = buildUrl + "/${buildName}.loader.js?v=" + buildVersion;`
  );

  html = html
    .replace(
      /^\s*dataUrl: [^\n]+\.data\.unityweb[^\n]+,$/m,
      `        dataUrl: buildUrl + "/${buildName}.data.unityweb?v=" + buildVersion,`
    )
    .replace(
      /^\s*frameworkUrl: [^\n]+\.framework\.js\.unityweb[^\n]+,$/m,
      `        frameworkUrl: buildUrl + "/${buildName}.framework.js.unityweb?v=" + buildVersion,`
    )
    .replace(
      /^\s*codeUrl: [^\n]+\.wasm\.unityweb[^\n]+,$/m,
      `        codeUrl: buildUrl + "/${buildName}.wasm.unityweb?v=" + buildVersion,`
    );

  html = html.replace(
    /^\s*canvas\.style\.background = [^\n]+\.jpg[^\n]+;$/m,
    `      canvas.style.background = "url('" + buildUrl + "/${buildName}.jpg?v=" + buildVersion + "') center / cover";`
  );

  html = html.replace(
    /canvas\.style\.width = "960px";\s*canvas\.style\.height = "600px";/,
    'canvas.style.width = "100%";\n        canvas.style.height = "100%";'
  );

  if (!html.includes("window.__forceUnityResize")) {
    html = html.replace(
      /(\s*document\.querySelector\("#unity-loading-bar"\)\.style\.display = "block";)/,
      `
      function forceUnityResize() {
        canvas.style.width = "100%";
        canvas.style.height = "100%";
        window.dispatchEvent(new Event("resize"));
      }

      window.__forceUnityResize = forceUnityResize;

      if (window.ResizeObserver) {
        new ResizeObserver(forceUnityResize).observe(document.documentElement);
      }

$1`
    );
  }

  html = html.replace(
    /document\.querySelector\("#unity-loading-bar"\)\.style\.display = "none";/,
    'document.querySelector("#unity-loading-bar").style.display = "none";\n                forceUnityResize();'
  );

  write(indexPath, html);
}

function patchUnityStyle() {
  const stylePath = path.join(targetBuild, "TemplateData", "style.css");
  assertExists(stylePath, "Unity WebGL style");

  const markerStart = "/* codex-responsive-webgl-start */";
  const markerEnd = "/* codex-responsive-webgl-end */";
  const responsivePatch = `${markerStart}
html,
body {
  width: 100%;
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden;
  background: #08050d;
}

#unity-container {
  position: fixed !important;
  inset: 0 !important;
  width: 100% !important;
  height: 100% !important;
}

#unity-container.unity-desktop,
#unity-container.unity-mobile {
  left: 0 !important;
  top: 0 !important;
  transform: none !important;
}

#unity-canvas {
  display: block;
  width: 100% !important;
  height: 100% !important;
}

#unity-footer {
  display: none;
}
${markerEnd}`;

  let css = read(stylePath);
  const existingPatch = new RegExp(`${markerStart}[\\s\\S]*?${markerEnd}\\n?`, "m");
  css = css.replace(existingPatch, "").trimEnd();
  write(stylePath, `${css}\n\n${responsivePatch}\n`);
}

function runVercelDeploy() {
  const result = spawnSync("vercel", ["--prod", "--yes"], {
    cwd: hostRoot,
    stdio: "inherit",
  });

  if (result.error) {
    fail(`failed to run vercel: ${result.error.message}`);
  }

  if (result.status !== 0) {
    fail(`vercel exited with ${result.status}`);
  }
}

function makeBuildVersion() {
  if (process.env.BUILD_VERSION) {
    return process.env.BUILD_VERSION;
  }

  const stamp = new Date().toISOString().replace(/[-:T.Z]/g, "").slice(0, 14);
  return `webgl-${stamp}`;
}

copyBuildOutput();

const buildName = detectBuildName(path.join(targetBuild, "Build"));
const buildVersion = makeBuildVersion();

patchUnityIndex(buildName, buildVersion);
patchUnityStyle();

console.log(`publish-webgl: copied ${sourceBuild} -> ${targetBuild}`);
console.log(`publish-webgl: patched Unity WebGL shell for ${buildName} (${buildVersion})`);

if (shouldDeploy) {
  runVercelDeploy();
} else {
  console.log("publish-webgl: skipped Vercel deploy (--no-deploy)");
}
