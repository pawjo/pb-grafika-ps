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


let mouseX = 0;
let mouseY = 0;
let currentElement = null;
let currentElementShape = null;
let currentAction = "none";
let selectedShape = null;
let selectedTool = null;

let scaleCurrentActions = null;

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
    mouseX = e.offsetX;
    mouseY = e.offsetY;

    switch (selectedShape) {
        case shapes.rectangle:
            startDrawRect(mouseX, mouseY);
            break;
        case shapes.circle:
            startDrawCircle(mouseX, mouseY);
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
    currentElement = document.createElementNS(svgns, "circle");
    currentElement.setAttribute("cx", startX);
    currentElement.setAttribute("cy", startY);
    currentElement.setAttribute("r", 0);
    finalizeSvgElement();
}

function createScalePoint(x, y, action) {
    let scalePoint = document.createElementNS(svgns, "circle");
    scalePoint.setAttribute("cx", x);
    scalePoint.setAttribute("cy", y);
    scalePoint.setAttribute("r", 5);
    scalePoint.classList.add("scale-point");
    svg.appendChild(scalePoint);

    scalePoint.addEventListener("mousedown", e => {
        e.stopPropagation();
        activeScalePointAction = action;
        currentAction = tools.scale;
    });

    return scalePoint;
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
    const dX = offsetX - mouseX;
    const dY = offsetY - mouseY;
    const dR = Math.sqrt(pow2(dX) + pow2(dY));
    circle.setAttribute("r", dR);
}

function drawing(e) {
    switch (currentElementShape) {
        case shapes.rectangle:
            resizeRect(currentElement, e.movementX, e.movementY);
            break;
        case shapes.circle:
            resizeCircle(currentElement, e.offsetX, e.offsetY);
            break;
    }
}

function moving(dX, dY) {
    switch (currentElementShape) {
        case shapes.rectangle:
            translateAttribute(currentElement, "x", dX);
            translateAttribute(currentElement, "y", dY);
            break;
        case shapes.circle:
            translateAttribute(currentElement, "cx", dX);
            translateAttribute(currentElement, "cy", dY);
            break;
    }
}

function checkIfIsNear(a, b, tolerance) {
    return Math.abs(a - b) > tolerance;
}

function scaling(e) {
    // const eventX = e.offsetX;
    // const eventY = e.offsetY;
    // const dX = e.movementX;
    // const dY = e.movementY;
    // switch (currentElementShape) {
    //     case shapes.rectangle:
    //         const x = currentElement.getAttribute("x");
    //         const y = currentElement.getAttribute("y");
    //         const width = currentElement.getAttribute("width");
    //         const height = currentElement.getAttribute("height");

    //         if (checkIfIsNear(x, eventX, 5) && checkIfIsNear(y, eventY, 5)) { //top left
    //             translateAttribute(currentElement, "x", dX);
    //             translateAttribute(currentElement, "y", dY);
    //             translateAttribute(currentElement, "width", -dX);
    //             translateAttribute(currentElement, "height", -dY);
    //         }
    //         // else if (checkIfIsNear(x + width, eventX, 5) &&)
    //         //     break;
    // }
}

function startManipulateRect(e) {
    currentElementShape = shapes.rectangle;
    if (selectedTool === tools.move) {
        startMove(e, shapes.rectangle);
    }
    else if (selectedTool === tools.scale) {
        console.log(e);
        currentAction === tools.scale;
        const offsetX = e.offsetX;
        const offsetY = e.offsetY;
        const x = e.target.getAttribute("x");
        const y = e.target.getAttribute("y");
        const width = e.target.getAttribute("width");
        const height = e.target.getAttribute("height");

        // scalePoints.push(createScalePoint(x, y, moveTopLeftCorner));
    }
}

function moveTopLeftCorner(dX, dY) {
    translateAttribute(currentElement, "x", dX);
    translateAttribute(currentElement, "y", dY);
    translateAttribute(currentElement, "width", -dX);
    translateAttribute(currentElement, "height", -dY);
}

function startMoveCircle(e) {
    startMove(e, shapes.circle);
}

function startMove(e, shape) {
    e.preventDefault();
    currentElement = e.target;
    currentAction = tools.move;
    currentElementShape = shape;
}

function stopDraw() {
    let actionForSpecifiedType = null;
    switch (currentElementShape) {
        case shapes.rectangle:
            actionForSpecifiedType = startManipulateRect;
            break;
        case shapes.circle:
            actionForSpecifiedType = startMoveCircle;
            break;
    }

    currentElement.addEventListener("mousedown", e => {
        e.stopPropagation();
        currentElement = e.target;
        actionForSpecifiedType(e);
    });
}

function onMouseDown(e) {
    // if (scalePoints.length > 0) {
    //     scalePoints.forEach(x => x.remove());
    //     scalePoints = [];
    // }

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
            scaling(e);
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