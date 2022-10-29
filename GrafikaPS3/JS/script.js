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

function showElement(id) {
    const panels = document.getElementsByClassName("panel");
    for (let element of panels) {
        element.style.display = "none";
    }
    const element = document.getElementById(id);
    element.style.display = "grid";
}

// function onVaueChange(e,inputToUpdateId){
//     const inputToUpdate = document.getElementById(inputToUpdateId);
//     inputToUpdate.value =  e.target.value;
// }

function onValueChange(e) {
    const target = e.target;
    if (target.value < 0) {
        target.value = 0;
    }

    checkSibling(target.nextElementSibling, target.value);
    checkSibling(target.previousElementSibling, target.value);

    setColorFromRGB();
}

function checkSibling(sibling, value) {
    if (!sibling)
        return;
    if (sibling.tagName.toLowerCase() === "input") {
        sibling.value = value;
    }
}

function addListeners() {
    const colorInputs = document.getElementsByClassName("color-input");

    for (let input of colorInputs) {
        input.setAttribute("min", "1");
        input.setAttribute("max", "255");
        input.setAttribute("value", "0");
        input.addEventListener("change", onValueChange);
    }
}

function getRGBValues() {
    const r = document.getElementById("rgb-input-r-field").value;
    const g = document.getElementById("rgb-input-g-field").value;
    const b = document.getElementById("rgb-input-b-field").value;

    return { r: r, g: g, b: b };
}

function setColorFromRGB() {
    const values = getRGBValues();
    document.getElementById("rgb-color-panel").style.backgroundColor = `rgb(${values.r},${values.g},${values.b})`;

    const k = 1 - (Math.max(values.r, values.g, values.b) / 255);
    const c = (1 - values.r / 255 - k) / (1 - k);
    const m = (1 - values.g / 255 - k) / (1 - k);
    const y = (1 - values.b / 255 - k) / (1 - k);

    document.getElementById("rgb-cmyk-c").value = c;
    document.getElementById("rgb-cmyk-m").value = m;
    document.getElementById("rgb-cmyk-y").value = y;
    document.getElementById("rgb-cmyk-k").value = k;

    setCMYKFromRGB(values);
    setHSVFromRGB(values);
}

function setCMYKFromRGB(values) {
    const k = 1 - (Math.max(values.r, values.g, values.b) / 255);
    const c = (1 - values.r / 255 - k) / (1 - k);
    const m = (1 - values.g / 255 - k) / (1 - k);
    const y = (1 - values.b / 255 - k) / (1 - k);

    document.getElementById("rgb-cmyk-c").value = c;
    document.getElementById("rgb-cmyk-m").value = m;
    document.getElementById("rgb-cmyk-y").value = y;
    document.getElementById("rgb-cmyk-k").value = k;
}
function setHSVFromRGB(values) {
    const r = values.r / 255.0;
    const g = values.g / 255.0;
    const b = values.b / 255.0;

    const cmax = Math.max(r, Math.max(g, b)); // maximum of r, g, b
    const cmin = Math.min(r, Math.min(g, b)); // minimum of r, g, b
    const diff = cmax - cmin; // diff of cmax and cmin.
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

    document.getElementById("rgb-hsv-h").value = h;
    document.getElementById("rgb-hsv-s").value = s;
    document.getElementById("rgb-hsv-v").value = v;
}


render();
addListeners();
