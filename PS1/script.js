// const canvas = document.querySelector("canvas"),
const shapeBtns = document.querySelectorAll(".shape"),
    toolBtns = document.querySelectorAll(".tool"),
    fillColor = document.querySelector("#fill-color"),
    saveImg = document.querySelector(".save-img"),
    // ctx = canvas.getContext("2d");
    svg = document.querySelector("#workspace");

const svgns = "http://www.w3.org/2000/svg";

const x = "x";
const y = "y";
const width = "width";
const height = "height";

let mouseX = 0;
let mouseY = 0;
let currentRect = null;
let currentAction = "none";
let selectedShape = "rectangle";
let selectedTool = "draw";

function setSelectedShape(shape) {
    selectedShape = shape;
}

function setSelectedTool(tool) {
    selectedTool = tool;
}

function addEventListenersForButtons(buttons, parent, activeClass, setAction) {
    buttons.forEach(btn => {
        btn.addEventListener("click", () => { // adding click event to all tool option
            // removing active class from the previous option and adding on current clicked option
            const selector = `.${parent} .${activeClass}`;
            const element = document.querySelector(selector);
            if (element) {
                element.classList.remove(activeClass);
            }
            btn.classList.add(activeClass);
            // selectedTool = btn.id;
            setAction(btn.id);
        });
    });
}

addEventListenersForButtons(shapeBtns, "options", "activeShape", setSelectedShape);
addEventListenersForButtons(toolBtns, "options", "activeTool", setSelectedTool);

function startDraw(e) {
    currentAction = "draw";
    mouseX = e.offsetX;
    mouseY = e.offsetY;

    startDrawRect(mouseX, mouseY);
}

function startDrawRect(startX, startY) {
    currentRect = document.createElementNS(svgns, "rect");
    currentRect.setAttribute(x, startX);
    currentRect.setAttribute(y, startY);
    currentRect.setAttribute(width, 0);
    currentRect.setAttribute(height, 0);
    currentRect.classList.add("rect");
    svg.appendChild(currentRect);
}

function translateAttribute(element, attributeName, difference) {
    const currentValue = parseInt(element.getAttribute(attributeName));
    element.setAttribute(attributeName, currentValue + difference);
}

function resizeRect(rect, dX, dY) {
    // const currentWidth = parseInt(rect.getAttribute(width));
    // const currentHeight = parseInt(rect.getAttribute(height));
    // currentRect.setAttribute(width, currentWidth + dX);
    // currentRect.setAttribute(height, currentHeight + dY);

    translateAttribute(rect, width, dX);
    translateAttribute(rect, height, dY);
}

function moveElement(element, dX, dY) {
    translateAttribute(element, x, dX);
    translateAttribute(element, y, dY);
}


function startMoveRect(e) {
    e.preventDefault();
    currentRect = e.target;
    currentAction = "move";
}

function stopDraw() {
    currentRect.addEventListener("mousedown", startMoveRect);
}

function onMouseDown(e) {
    if (selectedTool === "draw") {
        startDraw(e);
    }
}

function onMouseMove(e) {

    switch (currentAction) {
        case "none":
            return;
        case "draw":
            resizeRect(currentRect, e.movementX, e.movementY);
            break;
        case "move":
            moveElement(currentRect, e.movementX, e.movementY);
            break;
    }

    // } else if(selectedTool === "rectangle"){
    // } else if(selectedTool === "circle"){
    //     drawCircle(e);
    // } else if(selectedTool === "triangle"){
    //     drawTriangle(e);
    // } else if(selectedTool === "line"){
    //     drawLine(e);
    // }
}

function onMouseUp() {
    switch (selectedTool) {
        case "draw":
            stopDraw();
            break;
    }

    currentRect = null;
    currentAction = "none";
}

svg.addEventListener("mousedown", onMouseDown);
svg.addEventListener("mousemove", onMouseMove);
svg.addEventListener("mouseup", onMouseUp);

// const setCanvasBackground = () => {
//     // setting whole canvas background to white, so the downloaded img background will be white
//     ctx.fillStyle = "#fff";
//     ctx.fillRect(0, 0, canvas.width, canvas.height);
//      // setting fillstyle back to the selectedColor, it'll be the brush color
// }


// window.addEventListener("load", () => {
//     // setting canvas width/height.. offsetwidth/height returns viewable width/height of an element
//     canvas.width = canvas.offsetWidth;
//     canvas.height = canvas.offsetHeight;
//     setCanvasBackground();
// });

// const drawRect = (e) => {
//         return ctx.strokeRect(e.offsetX, e.offsetY, prevMouseX - e.offsetX, prevMouseY - e.offsetY);
// }

// const drawCircle = (e) => {
//     ctx.beginPath(); // creating new path to draw circle
//     // getting radius for circle according to the mouse pointer
//     let radius = Math.sqrt(Math.pow((prevMouseX - e.offsetX), 2) + Math.pow((prevMouseY - e.offsetY), 2));
//     ctx.arc(prevMouseX, prevMouseY, radius, 0, 2 * Math.PI); // creating circle according to the mouse pointer
//     fillColor.checked ? ctx.fill() : ctx.stroke(); // if fillColor is checked fill circle else draw border circle
// }

// const drawTriangle = (e) => {
//     ctx.beginPath(); // creating new path to draw circle
//     ctx.moveTo(prevMouseX, prevMouseY); // moving triangle to the mouse pointer
//     ctx.lineTo(e.offsetX, e.offsetY); // creating first line according to the mouse pointer
//     ctx.lineTo(prevMouseX * 2 - e.offsetX, e.offsetY); // creating bottom line of triangle
//     ctx.closePath(); // closing path of a triangle so the third line draw automatically
//     fillColor.checked ? ctx.fill() : ctx.stroke(); // if fillColor is checked fill triangle else draw border
// }

// const drawLine = (e) => {
//     ctx.beginPath(); // creating new path to draw circle
//     ctx.moveTo(prevMouseX, prevMouseY); // moving triangle to the mouse pointer
//     ctx.lineTo(e.offsetX, e.offsetY); // creating first line according to the mouse pointer
//    // creating bottom line of triangle
//     ctx.closePath(); // closing path of a triangle so the third line draw automatically
//     fillColor.checked ? ctx.fill() : ctx.stroke(); // if fillColor is checked fill triangle else draw border
// }

// const startDraw = (e) => {
//     isDrawing = true;
//     prevMouseX = e.offsetX; // passing current mouseX position as prevMouseX value
//     prevMouseY = e.offsetY; // passing current mouseY position as prevMouseY value
//     ctx.beginPath(); // creating new path to draw
//     ctx.lineWidth = brushWidth; // passing brushSize as line width
//     // copying canvas data & passing as snapshot value.. this avoids dragging the image
//     snapshot = ctx.getImageData(0, 0, canvas.width, canvas.height);
// }

// const drawing = (e) => {
//     if(!isDrawing) return; // if isDrawing is false return from here
//     ctx.putImageData(snapshot, 0, 0); // adding copied canvas data on to this canvas

//     if(selectedTool === "pencil") {
//         // if selected tool is eraser then set strokeStyle to white
//         // to paint white color on to the existing canvas content else set the stroke color to selected color
//         ctx.strokeStyle = selectedTool === "eraser" ? "#fff" : selectedColor;
//         ctx.lineTo(e.offsetX, e.offsetY); // creating line according to the mouse pointer
//         ctx.stroke(); // drawing/filling line with color
//     } else if(selectedTool === "rectangle"){
//         drawRect(e);
//     } else if(selectedTool === "circle"){
//         drawCircle(e);
//     } else if(selectedTool === "triangle"){
//         drawTriangle(e);
//     } else if(selectedTool === "line"){
//         drawLine(e);
//     }
// }



// saveImg.addEventListener("click", () => {
//     const link = document.createElement("a"); // creating <a> element
//     link.download = `${Date.now()}.jpg`; // passing current date as link download value
//     link.href = canvas.toDataURL(); // passing canvasData as link href value
//     link.click(); // clicking link to download image
// });