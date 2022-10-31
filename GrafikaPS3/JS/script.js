var scene = new THREE.Scene();
var camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 1, 1000);
var renderer = new THREE.WebGLRenderer();

renderer.setSize(700, 400);
renderer.setClearColor(0xffffff);
const mainContainer = document.getElementById("main-container");
const canvas = renderer.domElement;
canvas.id = "rgb-cube-panel";
canvas.style.display = "none";
canvas.classList.add("panel");
mainContainer.appendChild(canvas);

var contorls = new THREE.OrbitControls(camera, renderer.domElement);
var geom = new THREE.BoxGeometry(1, 1, 1);
var faceIndices = ['a', 'b', 'c'];
var vertexIndex;
var point;

const activeClassName = "active-panel";

geom.faces.forEach(function (face) {
    for (var i = 0; i < 3; i++) {

        vertexIndex = face[faceIndices[i]];
        point = geom.vertices[vertexIndex];

        color = new THREE.Color(
            point.x + 0.5,
            point.y + 0.5,
            point.z + 0.5
        );

        face.vertexColors[i] = color;
    }
});

var mat = new THREE.MeshBasicMaterial({
    vertexColors: THREE.VertexColors
});

var cube = new THREE.Mesh(geom, mat);

scene.add(cube);
camera.position.z = 5;

function render() {
    requestAnimationFrame(render);
    renderer.render(scene, camera);
};

// function showElement(id) {
//     const panels = document.getElementsByClassName("panel");
//     for (let element of panels) {
//         element.style.display = "none";
//     }
//     const element = document.getElementById(id);
//     element.style.display = "grid";
// }

function showElement(id) {
    const activePanel = document.getElementsByClassName(activeClassName)[0];
    activePanel.style.display = "none";
    activePanel.classList.remove(activeClassName);
    const element = document.getElementById(id);
    element.style.display = "grid";
    element.classList.add(activeClassName);
}

// function onVaueChange(e,inputToUpdateId){
//     const inputToUpdate = document.getElementById(inputToUpdateId);
//     inputToUpdate.value =  e.target.value;
// }

function onValueChange(e) {
    const target = e.target;
    checkAndFixValue(target);

    const activePanel = document.getElementsByClassName(activeClassName)[0];

    switch (activePanel.id) {
        case "rgb-panel":
            setColorFromRGB();
            break;
        case "cmyk-panel":
            setColorFromCMYK();
            break;
        case "hsv-panel":
            setColorFromHSV();
            break;
    }
}

function checkSibling(sibling, value) {
    if (!sibling)
        return;
    if (sibling.tagName.toLowerCase() === "input") {
        sibling.value = value;
    }
}

function addListeners(controlClassName, max) {
    const colorInputs = document.getElementsByClassName(controlClassName);

    for (let input of colorInputs) {
        input.setAttribute("min", "0");
        input.setAttribute("max", max);
        input.setAttribute("value", "0");
        input.addEventListener("change", onValueChange);
    }
}

function getAndFixValue(id, min, max) {
    const control = document.getElementById(id);

    if (!control) {
        return NaN;
    }

    if (control.value < min) {
        control.value = min;
    }
    else if (control.value > max) {
        control.value = max;
    }

    checkSibling(control.nextElementSibling, control.value);
    checkSibling(control.previousElementSibling, control.value);

    return control.value;
}

function checkAndFixValue(control) {
    const min = parseInt(control.getAttribute("min"));
    const max = parseInt(control.getAttribute("max"));

    if (control.value < min) {
        control.value = min;
    }
    else if (control.value > max) {
        control.value = max;
    }

    checkSibling(control.nextElementSibling, control.value);
    checkSibling(control.previousElementSibling, control.value);
}

function getRGBValues() {
    const r = document.getElementById("rgb-input-r-field").value;
    const g = document.getElementById("rgb-input-g-field").value;
    const b = document.getElementById("rgb-input-b-field").value;

    return { r: r, g: g, b: b };
}

function setOutput(id, value) {
    const control = document.getElementById(id);
    control.value = Math.round(value);
}


function setCMYKOutput(values, idPrefix) {
    const k = 1 - (Math.max(values.r, values.g, values.b) / 255);
    let c = 0;
    let m = 0;
    let y = 0;

    if (k < 1) {
        c = (1 - values.r / 255 - k) / (1 - k) * 100;
        m = (1 - values.g / 255 - k) / (1 - k) * 100;
        y = (1 - values.b / 255 - k) / (1 - k) * 100;
    }

    setOutput(idPrefix + "-cmyk-c", c);
    setOutput(idPrefix + "-cmyk-m", m);
    setOutput(idPrefix + "-cmyk-y", y);
    setOutput(idPrefix + "-cmyk-k", k * 100);
}

function setHSVOutput(values, idPrefix) {
    const r = values.r / 255.0;
    const g = values.g / 255.0;
    const b = values.b / 255.0;

    const cmax = Math.max(r, Math.max(g, b));
    const cmin = Math.min(r, Math.min(g, b));
    const diff = cmax - cmin;
    const s = cmax === 0 ? 0 : diff / cmax * 100;
    const v = cmax * 100;
    let h = -1;

    switch (cmax) {
        case cmin:
            h = 0;
            break;
        case r:
            h = (60 * ((g - b) / diff) + 360) % 360;
            break;
        case g:
            h = (60 * ((b - r) / diff) + 120) % 360;
            break;
        case b:
            h = (60 * ((r - g) / diff) + 240) % 360;
            break;
    }

    setOutput(idPrefix + "-hsv-h", h);
    setOutput(idPrefix + "-hsv-s", s);
    setOutput(idPrefix + "-hsv-v", v);
}

function setRGBOutput(values, idPrefix) {
    setOutput(idPrefix + "-rgb-r", values.r);
    setOutput(idPrefix + "-rgb-g", values.g);
    setOutput(idPrefix + "-rgb-b", values.b);
}

function setColorPanel(values, idPrefix) {
    const panel = document.getElementById(idPrefix + "-color-panel");
    panel.style.backgroundColor = `rgb(${values.r},${values.g},${values.b})`;
}

function setColorFromRGB() {
    const values = getRGBValues();
    setColorPanel(values, "rgb");

    // const k = 1 - (Math.max(values.r, values.g, values.b) / 255);
    // const c = (1 - values.r / 255 - k) / (1 - k);
    // const m = (1 - values.g / 255 - k) / (1 - k);
    // const y = (1 - values.b / 255 - k) / (1 - k);

    // document.getElementById("rgb-cmyk-c").value = c;
    // document.getElementById("rgb-cmyk-m").value = m;
    // document.getElementById("rgb-cmyk-y").value = y;
    // document.getElementById("rgb-cmyk-k").value = k;

    setCMYKOutput(values, "rgb");
    setHSVOutput(values, "rgb");
}

function getRGBFromCMYK(c, m, y, k) {
    const kDiff = 1 - k / 100;
    const r = 255 * (1 - c / 100) * kDiff;
    const g = 255 * (1 - m / 100) * kDiff;
    const b = 255 * (1 - y / 100) * kDiff;
    return { r: r, g: g, b: b };
}

function getRGBFromHSV(h, s, v) {
    const normalizedH = h == 360 ? 0 : h / 60;
    const fract = normalizedH - Math.floor(normalizedH);
    const roundedH = Math.round(normalizedH);
    const normalizedS = s / 100;
    const normalizedV = v / 100;
    const p = normalizedV * (1 - normalizedS);
    const q = normalizedV * (1 - (normalizedS * fract));
    const t = normalizedV * (1 - (normalizedS * (1 - fract)));

    let r = 0, g = 0, b = 0;

    switch (roundedH) {
        case 0:
            r = normalizedV;
            g = t;
            b = p;
            break;
        case 1:
            r = q;
            g = normalizedV;
            b = p;
            break;
        case 2:
            r = p;
            g = normalizedV;
            b = t;
            break;

        case 3:
            r = p;
            g = q;
            b = normalizedV;
            break;
        case 4:
            r = t;
            g = p;
            b = normalizedV;
            break;
        case 5:
            r = normalizedV;
            g = p;
            b = q;
            break;
    }

    const result = { r: r * 255, g: g * 255, b: b * 255 };
    return result;
}

function setColorFromCMYK() {
    const c = document.getElementById("cmyk-input-c-field").value;
    const m = document.getElementById("cmyk-input-m-field").value;
    const y = document.getElementById("cmyk-input-y-field").value;
    const k = document.getElementById("cmyk-input-k-field").value;

    const rgbValues = getRGBFromCMYK(c, m, y, k);
    setColorPanel(rgbValues, "cmyk");
    setHSVOutput(rgbValues, "cmyk");
    setRGBOutput(rgbValues, "cmyk");
}

function setColorFromHSV() {
    const h = document.getElementById("hsv-input-h-field").value;
    const s = document.getElementById("hsv-input-s-field").value;
    const v = document.getElementById("hsv-input-v-field").value;

    const rgbValues = getRGBFromHSV(h, s, v);
    setColorPanel(rgbValues, "hsv");
    setRGBOutput(rgbValues, "hsv");
    setCMYKOutput(rgbValues, "hsv");
}

render();
addListeners("rgb-color-input", 255);
addListeners("percent-color-input", 100);
addListeners("degres-color-input", 360);
setColorFromRGB();
setColorFromCMYK();
setColorFromHSV();