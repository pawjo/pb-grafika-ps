const shapeBtns = document.querySelectorAll(".shape"),
    toolBtns = document.querySelectorAll(".tool"),
    fillColor = document.querySelector("#fill-color"),
    saveImg = document.querySelector(".save-img"),
    svg = document.querySelector("#workspace");

const svgns = "http://www.w3.org/2000/svg";

const width = "width";
const height = "height";
const svgElementClassName = "svg-element";

const shapes = {
    line: "line",
    rectangle: "rectangle",
    circle: "circle",
    triangle: "triangle"
};

const tools = {
    draw: "draw",
    move: "move",
    scale: "scale"
};

const scalingTypes = {
    rectTop: "rectTop",
    rectTopLeft: "rectTopLeft",
    rectTopRight: "rectTopRight",
    rectBottom: "rectBottom",
    rectBottomLeft: "rectBottomLeft",
    rectBottomRight: "rectBottomRight",
    rectLeft: "rectLeft",
    rectRight: "rectRight",
    circle: "circle"
};


let mouseX = 0;
let mouseY = 0;
let currentElement = null;
let currentElementShape = null;
let currentAction = "none";
let selectedShape = null;
let selectedTool = null;

let currentCircleX = null;
let currentCircleY = null;

let currentScalingType = null;

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
    currentElementShape = selectedShape;
    const startX = e.offsetX;
    const startY = e.offsetY;

    switch (selectedShape) {
        case shapes.rectangle:
            startDrawRect(startX, startY);
            break;
        case shapes.circle:
            startDrawCircle(startX, startY);
            break;
    }
}

function finalizeSvgElement() {
    currentElement.classList.add(svgElementClassName);
    svg.appendChild(currentElement);
}

function startDrawRect(startX, startY) {
    currentElement = document.createElementNS(svgns, "rect");
    currentElement.setAttribute("x", startX);
    currentElement.setAttribute("y", startY);
    currentElement.setAttribute("width", 0);
    currentElement.setAttribute("height", 0);
    finalizeSvgElement();
}

function startDrawCircle(startX, startY) {
    currentCircleX = startX;
    currentCircleY = startY;

    currentElement = document.createElementNS(svgns, "circle");
    currentElement.setAttribute("cx", startX);
    currentElement.setAttribute("cy", startY);
    currentElement.setAttribute("r", 0);
    finalizeSvgElement();
}

function translateAttribute(element, attributeName, difference) {
    const currentValue = parseInt(element.getAttribute(attributeName));
    element.setAttribute(attributeName, currentValue + difference);
}

function resizeRect(rect, dX, dY) {
    translateAttribute(rect, "width", dX);
    translateAttribute(rect, "height", dY);
}

function pow2(val) {
    return Math.pow(val, 2);
}

function resizeCircle(circle, offsetX, offsetY) {
    const dX = offsetX - currentCircleX;
    const dY = offsetY - currentCircleY;
    const dR = Math.sqrt(pow2(dX) + pow2(dY));
    circle.setAttribute("r", dR);
}

function drawing(e) {
    switch (currentElement.tagName) {
        case "rect":
            resizeRect(currentElement, e.movementX, e.movementY);
            break;
        case "circle":
            resizeCircle(currentElement, e.offsetX, e.offsetY);
            break;
    }
}

function moving(dX, dY) {
    switch (currentElement.tagName) {
        case "rect":
            translateAttribute(currentElement, "x", dX);
            translateAttribute(currentElement, "y", dY);
            break;
        case "circle":
            translateAttribute(currentElement, "cx", dX);
            translateAttribute(currentElement, "cy", dY);
            break;
    }
}

function checkIfIsNear(a, b, tolerance) {
    const result = Math.abs(a - b) < tolerance;
    console.log(`compare result: ${result}`);
    return result;
}

function scaling(e) {
    const eventX = e.offsetX;
    const eventY = e.offsetY;
    const dX = e.movementX;
    const dY = e.movementY;

    switch (currentScalingType) {
        case scalingTypes.rectTop:
            translateAttribute(currentElement, "y", dY);
            translateAttribute(currentElement, "height", -dY);
            break;
        case scalingTypes.rectTopLeft:
            translateAttribute(currentElement, "x", dX);
            translateAttribute(currentElement, "y", dY);
            translateAttribute(currentElement, "width", -dX);
            translateAttribute(currentElement, "height", -dY);
            break;
        case scalingTypes.rectTopRight:
            translateAttribute(currentElement, "y", dY);
            translateAttribute(currentElement, "width", dX);
            translateAttribute(currentElement, "height", -dY);
            break;
        case scalingTypes.rectBottom:
            translateAttribute(currentElement, "height", dY);
            break;
        case scalingTypes.rectBottomLeft:
            translateAttribute(currentElement, "x", dX);
            translateAttribute(currentElement, "width", -dX);
            translateAttribute(currentElement, "height", dY);
            break;
        case scalingTypes.rectBottomRight:
            translateAttribute(currentElement, "width", dX);
            translateAttribute(currentElement, "height", dY);
            break;
        case scalingTypes.rectLeft:
            translateAttribute(currentElement, "x", dX);
            translateAttribute(currentElement, "width", -dX);
            break;
        case scalingTypes.rectRight:
            translateAttribute(currentElement, "width", dX);
            break;
        case scalingTypes.circle:
            resizeCircle(currentElement, eventX, eventY);
            break;
    }
}

function getIntAttribute(element, attributeName) {
    return parseInt(element.getAttribute(attributeName));
}

function prepareRectToScaling(e) {
    currentAction = tools.scale;
    const offsetX = e.offsetX;
    const offsetY = e.offsetY;
    const x = getIntAttribute(e.target, "x");
    const y = getIntAttribute(e.target, "y");
    const width = getIntAttribute(e.target, "width");
    const height = getIntAttribute(e.target, "height");
    const tolerance = 10;

    // top
    if (checkIfIsNear(y, offsetY, tolerance)) {
        if (checkIfIsNear(x, offsetX, tolerance)) {
            currentScalingType = scalingTypes.rectTopLeft;
        }
        else if (checkIfIsNear(x + width, offsetX, tolerance)) {
            currentScalingType = scalingTypes.rectTopRight;
        }
        else {
            currentScalingType = scalingTypes.rectTop;
        }
    }
    // bottom
    else if (checkIfIsNear(y + height, offsetY, tolerance)) {
        if (checkIfIsNear(x, offsetX, tolerance)) {
            currentScalingType = scalingTypes.rectBottomLeft;
        }
        else if (checkIfIsNear(x + width, offsetX, tolerance)) {
            currentScalingType = scalingTypes.rectBottomRight;
        }
        else {
            currentScalingType = scalingTypes.rectBottom;
        }
    }
    // sides
    else {
        if (checkIfIsNear(x, offsetX, tolerance)) {
            currentScalingType = scalingTypes.rectBottomLeft;
        }
        else if (checkIfIsNear(x + width, offsetX, tolerance)) {
            currentScalingType = scalingTypes.rectBottomRight;
        }
    }
}

function startMoveCircle(e) {
    startMove(e, shapes.circle);
}

function startMove(e, shape) {
    // e.preventDefault();
    // currentElement = e.target;
    currentAction = tools.move;
    // currentElementShape = shape;
}

function stopDraw() {
    currentElement.addEventListener("mousedown", onObjectMouseDown);
}

function onObjectMouseDown(e) {
    if (selectedTool === tools.draw) {
        return;
    }

    e.stopPropagation();
    currentElement = e.target;
    currentAction = selectedTool;

    if (currentAction === tools.scale)
        switch (e.target.tagName) {
            case "rect":
                prepareRectToScaling(e);
                break;
            case "circle":
                currentCircleX = getIntAttribute(currentElement, "cx");
                currentCircleY = getIntAttribute(currentElement, "cy");
                currentScalingType = scalingTypes.circle;
                break;
        }


}

function onMouseDown(e) {
    if (selectedTool === tools.draw && selectedShape) {
        startDraw(e);
    }
}

function onMouseMove(e) {
    if (!currentAction) {
        return;
    }

    switch (currentAction) {
        case tools.draw:
            drawing(e);
            break;
        case tools.move:
            moving(e.movementX, e.movementY);
            break;
        case tools.scale:
            console.log("switch");
            scaling(e);
            break;
    }
}

function onMouseUp() {
    switch (selectedTool) {
        case tools.draw:
            stopDraw();
            break;
    }

    // currentElement = null;
    currentAction = null;
}

svg.addEventListener("mousedown", onMouseDown);
svg.addEventListener("mousemove", onMouseMove);
svg.addEventListener("mouseup", onMouseUp);


// const drawTriangle = (e) => {
//     ctx.beginPath(); // creating new path to draw circle
//     ctx.moveTo(prevMouseX, prevMouseY); // moving triangle to the mouse pointer
//     ctx.lineTo(e.offsetX, e.offsetY); // creating first line according to the mouse pointer
//     ctx.lineTo(prevMouseX * 2 - e.offsetX, e.offsetY); // creating bottom line of triangle
//     ctx.closePath(); // closing path of a triangle so the third line draw automatically
//     fillColor.checked ? ctx.fill() : ctx.stroke(); // if fillColor is checked fill triangle else draw border
// }


// saveImg.addEventListener("click", () => {
//     const link = document.createElement("a"); // creating <a> element
//     link.download = `${Date.now()}.jpg`; // passing current date as link download value
//     link.href = canvas.toDataURL(); // passing canvasData as link href value
//     link.click(); // clicking link to download image
// });