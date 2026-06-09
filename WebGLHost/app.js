const stage = document.getElementById("stage");
const frame = document.getElementById("gameFrame");
const widthInput = document.getElementById("widthInput");
const heightInput = document.getElementById("heightInput");
const applySize = document.getElementById("applySize");
const reloadGame = document.getElementById("reloadGame");
const presets = Array.from(document.querySelectorAll(".preset"));

function setSize(width, height) {
  const nextWidth = Math.max(320, Math.min(1280, Number(width) || 390));
  const nextHeight = Math.max(320, Math.min(1280, Number(height) || 844));
  stage.style.setProperty("--stage-width", `${nextWidth}px`);
  stage.style.setProperty("--stage-height", `${nextHeight}px`);
  stage.style.setProperty("--stage-aspect", String(nextWidth / nextHeight));
  widthInput.value = String(nextWidth);
  heightInput.value = String(nextHeight);

  presets.forEach((button) => {
    button.classList.toggle("active", button.dataset.size === `${nextWidth}x${nextHeight}`);
  });

  requestAnimationFrame(notifyGameResize);
}

function notifyGameResize() {
  const gameWindow = frame.contentWindow;

  if (!gameWindow)
    return;

  if (typeof gameWindow.__forceUnityResize === "function") {
    gameWindow.__forceUnityResize();
    return;
  }

  gameWindow.dispatchEvent(new Event("resize"));
}

presets.forEach((button) => {
  button.addEventListener("click", () => {
    const [width, height] = button.dataset.size.split("x").map(Number);
    setSize(width, height);
  });
});

applySize.addEventListener("click", () => {
  setSize(widthInput.value, heightInput.value);
});

reloadGame.addEventListener("click", () => {
  const url = new URL(frame.src);
  url.searchParams.set("reload", String(Date.now()));
  frame.src = url.toString();
});

frame.addEventListener("load", notifyGameResize);
