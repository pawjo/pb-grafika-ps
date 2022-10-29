var scene = new THREE.Scene();
var camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 1, 1000);
var renderer = new THREE.WebGLRenderer();

renderer.setSize(400, 400);
const mainContainer = document.getElementById("main-container");
mainContainer.appendChild(renderer.domElement);

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

render();

function hideCanvas(){
    const canvas = document.getElementsByTagName("canvas")[0];
    canvas.style.display = "none";
}

function showCanvas(){
    const canvas = document.getElementsByTagName("canvas")[0];
    canvas.style.display = "block";
}

hideCanvas();