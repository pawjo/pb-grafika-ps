var scene = new THREE.Scene();
var camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 1, 1000);
var renderer = new THREE.WebGLRenderer();

renderer.setSize(700, 400);
renderer.setClearColor( 0xffffff );
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


render();
// hideElement();
