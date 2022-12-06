const shapeBtns = document.querySelectorAll(".shape"),
    toolBtns = document.querySelectorAll(".tool"),
    saveImg = document.querySelector(".save-img"),
    svg = document.querySelector("#workspace");

const svgns = "http://www.w3.org/2000/svg";

const width = "width";
const height = "height";
const svgElementClassName = "svg-element";

const tolerance = 10;

const textField = document.getElementById("text-field");

const bezierStride = 0.05;

const shapes = {
    line: "line",
    rectangle: "rectangle",
    circle: "circle",
    polygon: "polygon",
    bezier: "bezier"
};

const tools = {
    draw: "draw",
    move: "move",
    scale: "scale",
    pencil: "pencil",
    text: "text",
    drawPolygon: "drawPolygon",
    drawBezier: "draw-bezier",
    modifyBezier: "modify-bezier",
    moveBezier: "move-bezier",
    scaleBezier: "scale-bezier",
    openImage: "open-image",
    select: "select",
    toFront: "toFront",
    toBack: "toBack",
    forward: "forward",
    backward: "backward"
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
    circle: "circle",
    line1: "line1",
    line2: "line2",
    polygonPoint: "polygonPoint"
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

let currentTriangleAttributes = null;

let currentElementPoints = null;
let currentPolygonPointIndex = null;

let currentPolyline = null;

let isPolygonDrawing = false;

let currentBezierGroup = null;
let currentBezierPoints = null;
let bezierFactorsX = null;
let bezierFactorsY = null;

const bufferSize = 8;
let boundingRect = svg.getBoundingClientRect();
let path = null;
let strPath;
let buffer = [];

let brushWidth = 2;
const sizeSlider = document.querySelector("#size-slider");

function applyBrushChange(attributeName, value) {
    let element = currentBezierGroup ? currentBezierGroup.getElementsByTagName("polyline")[0] : null;

    if (!element && currentElement) {
        const tag = currentElement.tagName;
        element = (tag === "rect" || tag === "circle" || tag === "polygon") ? currentElement : null;
    }

    if (element)
        element.setAttribute(attributeName, value);
}

sizeSlider.addEventListener("change", () => {
    brushWidth = sizeSlider.value;
    applyBrushChange("stroke-width", brushWidth);
});

const changeColor = document.querySelector("#changeColor");

changeColor.addEventListener("change", () => {
    brushColor = changeColor.value;
    applyBrushChange("stroke", brushColor);
});


let checkBox = document.getElementById("fill-color");
let colorFill = "none";
let brushColor = "black";

let labelingArray = null;
let labels = null;
let linkedLabels = null;
let processedLabels = null;

function changeFill() {
    let fillColor = document.getElementById('fill-color').value;
    if (fillColor.checked == true) {
        colorFill = fillColor;
    } else {
        colorFill = "none";
    }

}

function setSelectedShape(shape) {
    selectedShape = shape;
}

function setSelectedTool(tool) {
    // switch (selectedTool) {
    //     case tools.drawBezier:
    //     case tools.modifyBezier:
    //     case tools.moveBezier:
    //     case tools.scaleBezier:
    //         break;
    //     default:
    //         currentBezierGroup = null;
    //         currentBezierPoints = null;
    //         break;
    // }
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
        case shapes.line:
            startDrawLine(startX, startY);
            break;
        case shapes.polygon:
            // startDrawTriangle(startX, startY);
            startDrawPolygon(startX, startY);
            break;
    }
}

function finalizeSvgElement() {
    // currentElement.classList.add(svgElementClassName);
    currentElement.setAttribute("fill", colorFill);
    currentElement.setAttribute("stroke", brushColor);
    currentElement.setAttribute("stroke-width", brushWidth);
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

function readPositionFromInput(parent) {
    const inputs = parent.getElementsByTagName("input");
    const x = inputs[0].value;
    const y = inputs[1].value;

    const circles = currentBezierGroup.getElementsByTagName("circle");
    // const point = circles.find(x=>x.getAttribute("point-id") == )
    console.log(el);
}

function onBezierPointControlChange(e) {
    const parent = e.target.parentElement;
    const inputs = parent.getElementsByTagName("input");
    const x = inputs[0].value;
    const y = inputs[1].value;
    const id = parent.getAttribute("point-id");
    const circles = currentBezierGroup.getElementsByTagName("circle");
    for (let i = 0; i < circles.length; i++) {
        const attr = circles[i].getAttribute("point-id");
        if (attr === id) {
            currentElement = circles[i];
            currentBezierPoints[i].x = x;
            currentBezierPoints[i].y = y;
            break;
        }
    }
    currentElement.setAttribute("cx", x);
    currentElement.setAttribute("cy", y);
    generateBezierCurve();
}

function createInput(value) {
    const input = document.createElement("input");
    const bezier = document.getElementById("bezier-points");
    input.classList.add("point");
    bezier.style.display = 'block';
    input.setAttribute("type", "number");
    input.value = value;
    input.addEventListener("change", onBezierPointControlChange);
    return input;
}


function createBezierPointControl(x, y, id) {
    const inputX = createInput(x);
    const inputY = createInput(y);
    const button = document.createElement("button");
    button.setAttribute("type", "button");
    button.innerText = "X";
    button.classList.add("close");
    button.onclick = () => {
        const circles = currentBezierGroup.getElementsByTagName("circle");
        for (let i = 0; i < circles.length; i++) {
            const attr = circles[i].getAttribute("point-id");
            if (attr == id) {
                currentElement = circles[i];
                currentElement.remove();
                getBezierControlById(id).remove();
                getBezierPointsFromCurrentGroup();
                generateBezierCurve();
                break;
            }
        }
    }

    const newControl = document.createElement("div")
    newControl.setAttribute("point-id", id);
    newControl.appendChild(inputX);
    newControl.appendChild(inputY);
    newControl.appendChild(button);
    const box = document.getElementById("bezier-points");
    box.appendChild(newControl);
}

function getBezierControlById(id) {
    const box = document.getElementById("bezier-points");
    const controls = box.getElementsByTagName("div");
    for (let i = 0; i < controls.length; i++) {
        if (controls[i].getAttribute("point-id") == id) {
            return controls[i];
        }
    }
    return null;
}

function updateBezierPointControl(x, y, id) {
    const control = getBezierControlById(id);
    const inputs = control.getElementsByTagName("input");
    inputs[0].value = x;
    inputs[1].value = y;
}

function deleteBezierPoint(id) {
    const control = getBezierControlById(id);
    control.remove();
}

function addPointToBezier(x, y) {
    const point = document.createElementNS(svgns, "circle");
    point.setAttribute("cx", x);
    point.setAttribute("cy", y);
    point.setAttribute("r", 5);
    point.setAttribute("fill", "#4A98F7");
    let id = 0;
    if (currentBezierPoints.length > 0) {
        id = currentBezierPoints[currentBezierPoints.length - 1].id + 1;
    }
    point.setAttribute("point-id", id);
    currentBezierGroup.appendChild(point);
    currentBezierPoints.push({ x: x, y: y, id: id });
    createBezierPointControl(x, y, id);
}

function getBezierPointsFromCurrentGroup() {
    const points = currentBezierGroup.getElementsByTagName("circle");
    currentBezierPoints = [];
    for (let i = 0; i < points.length; i++) {
        points[i].setAttribute("fill", "#4A98F7");
        const x = parseInt(points[i].getAttribute("cx"));
        const y = parseInt(points[i].getAttribute("cy"));
        const id = points[i].getAttribute("point-id");
        currentBezierPoints.push({
            x: x,
            y: y,
            id: id
        })
        createBezierPointControl(x, y, id);
    }
}

function removeChildren(parent) {
    while (parent.firstChild) {
        parent.removeChild(parent.firstChild);
    }
}

function unselectCurrentBezier() {
    const points = currentBezierGroup.getElementsByTagName("circle");
    for (let i = 0; i < points.length; i++) {
        points[i].removeAttribute("fill");
    }
    currentBezierGroup.classList.remove("current-element");
    currentBezierGroup = null;
    currentBezierPoints = null;
    const box = document.getElementById("bezier-points");
    removeChildren(box);
}

function startDrawBezier(startX, startY) {
    currentBezierGroup = document.createElementNS(svgns, "g");
    currentBezierGroup.classList.add("current-element");
    svg.appendChild(currentBezierGroup);
    currentBezierPoints = [];
    addPointToBezier(startX, startY);
}

function binomialCoefficient(n, k) {
    if (k == 1)
        return n;
    if (k == 0 || k == n)
        return 1;

    let result = 1;
    for (let i = k + 1; i <= n; i++)
        result *= i;
    for (let i = 2; i <= n - k; i++)
        result /= i;

    return result;
}

function generateBezierCurve() {
    const n = currentBezierPoints.length - 1;
    bezierFactorsX = [];
    bezierFactorsY = [];
    let tmpX = 0;
    let tmpY = 0;
    currentElementPoints = [];

    for (let i = 0; i <= n; i++) {
        const bc = binomialCoefficient(n, i);
        bezierFactorsX.push(bc * currentBezierPoints[i].x);
        bezierFactorsY.push(bc * currentBezierPoints[i].y);
    }

    for (let t = 0.0; t <= 1.001; t += bezierStride) {
        tmpX = 0;
        tmpY = 0;
        const diff = 1 - t;
        for (let i = 0; i <= n; i++) {
            tmpX += bezierFactorsX[i] * Math.pow(diff, n - i) * Math.pow(t, i);
            tmpY += bezierFactorsY[i] * Math.pow(diff, n - i) * Math.pow(t, i);
        }
        currentElementPoints.push({ x: tmpX, y: tmpY });
    }

    for (const child of currentBezierGroup.children) {
        if (child.tagName === 'polyline')
            child.remove();
    }

    const curve = document.createElementNS(svgns, "polyline");
    curve.setAttribute("points", getCurrentPolygonPointsToString());
    curve.setAttribute("fill", "none");
    curve.setAttribute("stroke", brushColor);
    curve.setAttribute("stroke-width", brushWidth);
    currentBezierGroup.appendChild(curve);
}

function drawBezierPoint(x, y) {
    addPointToBezier(x, y);
    if (currentBezierPoints.length < 3) {
        return;
    }

    generateBezierCurve();
}

function moveBezierPoint(dX, dY, id) {
    moving(dX, dY);
    const point = currentBezierPoints.find(x => x.id == id);
    point.x += dX;
    point.y += dY;
    updateBezierPointControl(point.x, point.y, id);
}

function setLineAttributesToStartPosition(startX, startY) {
    currentElement.setAttribute("x1", startX);
    currentElement.setAttribute("y1", startY);
    currentElement.setAttribute("x2", startX);
    currentElement.setAttribute("y2", startY);
}

function startDrawLine(startX, startY) {
    currentElement = document.createElementNS(svgns, "line");
    setLineAttributesToStartPosition(startX, startY);
    finalizeSvgElement();
}

function setCurrentTriangleAttributes(x1, y1, x2, y2, x3, y3) {
    currentTriangleAttributes = {
        x1: x1,
        y1: y1,
        x2: x2,
        y2: y2,
        x3: x3,
        y3: y3
    };
}

function addPointToCurrentPolygon(x, y) {
    currentElementPoints.push({ x: x, y: y });
}

function setCurrentPolygonAttributesFromCurrentElement() {
    const splitted = currentElement.getAttribute("points").split(/\,|\s/);
    const parsed = splitted.map(x => parseInt(x));
    currentElementPoints = [];
    for (let i = 0; i < parsed.length;) {
        addPointToCurrentPolygon(parsed[i++], parsed[i++]);
    }
}

function getCurrentTriangleAttributes() {
    return `${currentTriangleAttributes.x1},${currentTriangleAttributes.y1} `
        + `${currentTriangleAttributes.x2},${currentTriangleAttributes.y2} `
        + `${currentTriangleAttributes.x3},${currentTriangleAttributes.y3}`;
}

function getCurrentPolygonPointsToString() {
    let result = "";
    currentElementPoints.forEach(point => result += ` ${point.x},${point.y}`);
    return result.trimStart();
}

function setTriangleAttributes() {
    currentElement.setAttribute("points", getCurrentTriangleAttributes());
}

function startDrawTriangle(startX, startY) {
    currentElement = document.createElementNS(svgns, "polygon");
    setCurrentTriangleAttributes(startX, startY, startX, startY, startX + 50, startY + 50);
    setTriangleAttributes();
    finalizeSvgElement();
}

function drawPolygonPoint(x, y) {
    const firstPoint = currentElementPoints[0];
    if (checkIfIsNear(x, firstPoint.x, tolerance) && checkIfIsNear(y, firstPoint.y, tolerance)) {
        currentElement.remove();
        currentPolyline.remove();
        currentElement = document.createElementNS(svgns, "polygon");
        currentElement.setAttribute("points", getCurrentPolygonPointsToString());
        finalizeSvgElement();
        selectedTool = null;
        currentAction = null;
        // stopDraw();
    }
    else {
        addPointToCurrentPolygon(x, y);
        currentPolyline.setAttribute("points", getCurrentPolygonPointsToString());
        setLineAttributesToStartPosition(x, y);
    }
}

function startDrawPolygon(startX, startY) {
    currentElement = document.createElementNS(svgns, "polyline");
    finalizeSvgElement();
    currentPolyline = currentElement;
    startDrawLine(startX, startY);
    currentElementPoints = [];
    addPointToCurrentPolygon(startX, startY);
    selectedTool = tools.drawPolygon;
    currentAction = tools.drawPolygon;
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

function drawingTriangle(dX, dY) {
    currentTriangleAttributes.x2 += dX;
    currentTriangleAttributes.y2 += dY;
    setTriangleAttributes();
}

function drawingLine(dX, dY) {
    translateAttribute(currentElement, "x2", dX);
    translateAttribute(currentElement, "y2", dY);
}

function drawing(e) {

    switch (currentElement.tagName) {
        case "rect":
            translateAttribute(currentElement, "width", e.movementX);
            translateAttribute(currentElement, "height", e.movementY);
            break;
        case "circle":
            resizeCircle(currentElement, e.offsetX, e.offsetY);
            break;
        case "line":
            drawingLine(e.movementX, e.movementY);
            break;
        // case "polygon":
        //     currentTriangleAttributes.x2 += e.movementX;
        //     currentTriangleAttributes.y2 += e.movementY;
        //     setTriangleAttributes();
        //     break;
    }
}

function moving(dX, dY) {
    switch (currentElement.tagName) {
        case "rect":
        case "image":
            translateAttribute(currentElement, "x", dX);
            translateAttribute(currentElement, "y", dY);
            break;
        case "circle":
            translateAttribute(currentElement, "cx", dX);
            translateAttribute(currentElement, "cy", dY);
            break;
        case "line":
            translateAttribute(currentElement, "x1", dX);
            translateAttribute(currentElement, "y1", dY);
            translateAttribute(currentElement, "x2", dX);
            translateAttribute(currentElement, "y2", dY);
            break;
        case "polygon":
            currentElementPoints.forEach(element => {
                element.x += dX;
                element.y += dY;
            });
            currentElement.setAttribute("points", getCurrentPolygonPointsToString());
            break;
    }
}

function checkIfIsNear(a, b, tolerance) {
    const result = Math.abs(a - b) < tolerance;
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
        case scalingTypes.line1:
            translateAttribute(currentElement, "x1", dX);
            translateAttribute(currentElement, "y1", dY);
            break;
        case scalingTypes.line2:
            translateAttribute(currentElement, "x2", dX);
            translateAttribute(currentElement, "y2", dY);
            break;
        case scalingTypes.polygonPoint:
            const point = currentElementPoints[currentPolygonPointIndex];
            point.x += dX;
            point.y += dY;
            currentElement.setAttribute("points", getCurrentPolygonPointsToString());
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

// function stopDraw() {
//     currentElement.addEventListener("mousedown", onObjectMouseDown);
// }

function startScaleCurve(e) {
    const id = e.target.getAttribute("point-id");

}

function updateBrushSizeAndColor(element) {
    changeColor.value = element.getAttribute("stroke");
    sizeSlider.value = element.getAttribute("stroke-width");
}

function onObjectMouseDown(e) {
    if (selectedTool === tools.draw) {
        return;
    }

    e.stopPropagation();

    if (currentElement && e.target !== currentElement) {
        currentElement.classList.remove("current-element");
    }
    currentElement = e.target;

    if (!currentBezierGroup && currentElement.parentElement.tagName === "g") {
        currentBezierGroup = currentElement.parentElement;
        currentBezierGroup.classList.add("current-element");
        getBezierPointsFromCurrentGroup();
        const polyline = currentBezierGroup.getElementsByTagName("polyline")[0];
        updateBrushSizeAndColor(polyline);
    }
    else if (!currentBezierGroup) {
        currentElement.classList.add("current-element");
        updateBrushSizeAndColor(currentElement);
    }

    if (currentElement.tagName === "image" && currentElement.hasAttribute("area-percent")) {
        const area = parseInt(currentElement.getAttribute("area-percent"));
        const largest = parseInt(currentElement.getAttribute("largest-area-percent"));
        updateAreaDetails(area, largest);
    }

    currentAction = selectedTool;


    if (currentElement.tagName === "polygon") {
        setCurrentPolygonAttributesFromCurrentElement();
    }

    if (currentAction === tools.scale)
        switch (currentElement.tagName) {
            case "rect":
            case "image":
                prepareRectToScaling(e);
                break;
            case "circle":
                currentCircleX = getIntAttribute(currentElement, "cx");
                currentCircleY = getIntAttribute(currentElement, "cy");
                currentScalingType = scalingTypes.circle;
                break;
            case "line":
                const x1 = getIntAttribute(currentElement, "x1");
                const y1 = getIntAttribute(currentElement, "y1");
                const x2 = getIntAttribute(currentElement, "x2");
                const y2 = getIntAttribute(currentElement, "y2");

                if (checkIfIsNear(x1, e.offsetX, tolerance) && checkIfIsNear(y1, e.offsetY, tolerance)) {
                    currentScalingType = scalingTypes.line1;
                }
                else if (checkIfIsNear(x2, e.offsetX, tolerance) && checkIfIsNear(y2, e.offsetY, tolerance)) {
                    currentScalingType = scalingTypes.line2;
                }
                break;
            case "polygon":
                for (let i = 0; i < currentElementPoints.length; i++) {
                    if (checkIfIsNear(currentElementPoints[i].x, e.offsetX, tolerance)
                        && checkIfIsNear(currentElementPoints[i].y, e.offsetY, tolerance)) {
                        currentPolygonPointIndex = i;
                    }
                }
                currentScalingType = scalingTypes.polygonPoint;
                break;
        }
}

function unselectCurrentElement() {
    if (currentBezierGroup) {
        unselectCurrentBezier();
    }
    else if (currentElement) {
        currentElement.classList.remove("current-element");
        currentElement = null;
        currentPolygonPointIndex = 0;
    }
}

function onSvgMouseDown(e) {
    if (selectedTool === tools.pencil) {
        currentAction = tools.pencil;
        path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        // path.setAttribute("fill", fillColor);
        path.setAttribute("stroke", brushColor);
        // path.setAttribute("stroke-width", 5);
        buffer = [];
        let pt = getMousePosition(e);
        appendToBuffer(pt);
        // strPath = "M" + e.offsetX + " " + e.offsetY;
        strPath = "M" + pt.x + " " + pt.y;
        path.setAttribute("d", strPath);
        svg.appendChild(path);
    }
    else if (selectedTool === tools.text && textField.value.length > 0) {
        textField.style.display = "none";
        currentElement = document.createElementNS(svgns, "text");
        currentElement.setAttribute("x", e.offsetX);
        currentElement.setAttribute("y", e.offsetY);
        currentElement.innerHTML = textField.value;
        finalizeSvgElement();
        currentElement.setAttribute("stroke", "none");
        currentElement.setAttribute("fill", "black");
    }
    else if (selectedTool === tools.text) {
        textField.style.background = "white";
        textField.style.display = "block";
        textField.style.zIndex = 1000;
        textField.value = "";
        textField.style.position = "absolute";
    }
    else {
        unselectCurrentElement();
    }
}

function onMouseDown(e) {
    if (selectedTool === tools.drawPolygon) {
        drawPolygonPoint(e.offsetX, e.offsetY);
    }
    else if (selectedTool === tools.drawBezier && currentBezierGroup) {
        drawBezierPoint(e.offsetX, e.offsetY);
    }
    else if (selectedTool === tools.drawBezier) {
        unselectCurrentElement();
        startDrawBezier(e.offsetX, e.offsetY);
    }
    else if (selectedTool === tools.draw && selectedShape) {
        unselectCurrentElement();
        startDraw(e);
    }
    // else if (selectedTool === tools.scale && currentElement.tagName === "g" && e.target.tagName === "circle") {
    //     startScaleCurve(e);
    // }
    else if (e.target.tagName === "svg") {
        onSvgMouseDown(e);
    }
    else {
        onObjectMouseDown(e);
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
            break;
        case tools.pencil:
            if (path) {
                appendToBuffer(getMousePosition(e));
                updateSvgPath();
            }
            break;
        case tools.drawPolygon:
            drawingLine(e.movementX, e.movementY);
            break;
        case tools.modifyBezier:
            const id = currentElement.getAttribute("point-id");
            moveBezierPoint(e.movementX, e.movementY, id);
            generateBezierCurve();
            break;
        case tools.moveBezier:
            const points = currentBezierGroup.getElementsByTagName("circle");
            for (let i = 0; i < points.length; i++) {
                const id = points[i].getAttribute("point-id");
                currentElement = points[i];
                moveBezierPoint(e.movementX, e.movementY, id);
                generateBezierCurve();
            }
            break;
    }
}

function onMouseUp() {
    switch (selectedTool) {
        case tools.pencil:
            if (path) {
                path = null;
            }
        case tools.draw:
        case tools.move:
        case tools.scale:
        case tools.modifyBezier:
            currentAction = null;
            break;
        case tools.moveBezier:
            currentAction = null;
            currentElement = null;
            break;
    }
}

svg.addEventListener("mousedown", onMouseDown);
svg.addEventListener("mousemove", onMouseMove);
svg.addEventListener("mouseup", onMouseUp);


// pencil

var getMousePosition = function (e) {
    return {
        x: e.offsetX - boundingRect.left,
        y: e.offsetY - boundingRect.top
    }
    // return {
    //     x: e.offsetX,
    //     y: e.offsetY
    // }
};

var appendToBuffer = function (pt) {
    buffer.push(pt);
    while (buffer.length > bufferSize) {
        buffer.shift();
    }
};

// Calculate the average point, starting at offset in the buffer
var getAveragePoint = function (offset) {
    var len = buffer.length;
    if (len % 2 === 1 || len >= bufferSize) {
        var totalX = 0;
        var totalY = 0;
        var pt, i;
        var count = 0;
        for (i = offset; i < len; i++) {
            count++;
            pt = buffer[i];
            totalX += pt.x;
            totalY += pt.y;
        }
        return {
            x: totalX / count,
            y: totalY / count
        }
    }
    return null;
};

var updateSvgPath = function () {
    var pt = getAveragePoint(0);

    if (pt) {
        // Get the smoothed part of the path that will not change
        strPath += " L" + pt.x + " " + pt.y;

        // Get the last part of the path (close to the current mouse position)
        // This part will change if the mouse moves again
        var tmpPath = "";
        for (var offset = 2; offset < buffer.length; offset += 2) {
            pt = getAveragePoint(offset);
            tmpPath += " L" + pt.x + " " + pt.y;
        }

        // Set the complete current path coordinates
        path.setAttribute("d", strPath + tmpPath);
    }
};

function openSvg() {
    let input = document.createElement('input');
    input.type = 'file';
    input.onchange = _ => {
        const file = input.files[0];
        if (!file) {
            return;
        }
        var reader = new FileReader();
        reader.onload = function (e) {
            var contents = e.target.result;
            const startIndex = contents.indexOf("<svg");
            const svgContent = contents.substring(startIndex);
            const svgInnerStart = svgContent.indexOf(">") + 1;
            const svgInner = svgContent.substring(svgInnerStart, svgContent.length - 6);
            svg.innerHTML = svgInner;
            const svgStart = svgContent.substring(0, svgInnerStart);
            const indexOfStyle = svgStart.indexOf("style");
            if (indexOfStyle !== -1) {
                const styleStartIndex = indexOfStyle + 7;
                const styleEndIndex = svgStart.indexOf('"', styleStartIndex);
                const styleContent = svgStart.substring(styleStartIndex, styleEndIndex);
                if (styleContent !== "")
                    svg.setAttribute("style", styleContent);
            }
            else
                setBackground("");
        };
        reader.readAsText(file);
    };
    input.click();
}

function onLoadTmpImage(e) {
    unselectCurrentElement();
    let imageWidth = e.target.width;
    let imageHeight = e.target.height;

    const sketch = document.getElementById("sketch");
    const svgWidth = sketch.offsetWidth;
    const svgHeight = sketch.offsetHeight;

    if (imageWidth > svgWidth || imageHeight > svgHeight) {
        const factor = Math.max(imageWidth / svgWidth, imageHeight / svgHeight);
        imageWidth /= factor;
        imageHeight /= factor;
    }

    if (currentElement)
        currentElement.classList.remove("current-element");

    currentElement = document.createElementNS(svgns, "image");
    currentElement.setAttribute("href", e.target.src);
    currentElement.setAttribute("width", imageWidth);
    currentElement.setAttribute("height", imageHeight);
    currentElement.setAttribute("x", 0);
    currentElement.setAttribute("y", 0);
    currentElement.classList.add("current-element");
    svg.appendChild(currentElement);
}

function openImage() {
    let input = document.createElement('input');
    input.type = 'file';
    input.onchange = () => {
        const file = input.files[0];
        if (!file) {
            return;
        }

        const reader = new FileReader();
        reader.onload = e => {
            const tmpImage = new Image();
            tmpImage.onload = onLoadTmpImage;
            tmpImage.src = e.target.result;
        };

        reader.readAsDataURL(file);
    };
    input.click();
}

function saveSvg() {
    const result = prompt("Enter file name");
    if (result === null)
        return;

    if (result === "") {
        alert("File name cannot be empty");
        return;
    }

    const indexOfSuffix = result.indexOf(".svg");
    const fileName = indexOfSuffix === -1 ? result : result.substring(0, result.length - 4);

    const activeElements = document.getElementsByClassName("current-element");
    for (let i = 0; i < activeElements.length; i++) {
        activeElements[i].classList.remove("current-element");
    }
    svg.setAttribute("xmlns", "http://www.w3.org/2000/svg");
    var svgData = svg.outerHTML;
    var preface = '<?xml version="1.0" standalone="no"?>\r\n';
    var svgBlob = new Blob([preface, svgData], { type: "image/svg+xml;charset=utf-8" });
    var svgUrl = URL.createObjectURL(svgBlob);
    var downloadLink = document.createElement("a");
    downloadLink.href = svgUrl;
    downloadLink.download = fileName;
    document.body.appendChild(downloadLink);
    downloadLink.click();
    document.body.removeChild(downloadLink);
}

// start Screem

function start() {

    var container = document.getElementById('container');
    var start = document.getElementById('startScreen');
    console.log(start);

    container.style.display = 'flex';
    start.style.display = 'none';

}

function changecolor() {
    let color = document.getElementById('colorpicker').value;
    var workspace = document.getElementById('workspace');
    workspace.style.backgroundColor = color;
}

const getTransformParameters = (element) => {
    const transform = element.style.transform;
    let scale = 1,
        x = 0,
        y = 0;

    if (transform.includes("scale"))
        scale = parseFloat(transform.slice(transform.indexOf("scale") + 6));
    if (transform.includes("translateX"))
        x = parseInt(transform.slice(transform.indexOf("translateX") + 11));
    if (transform.includes("translateY"))
        y = parseInt(transform.slice(transform.indexOf("translateY") + 11));

    return { scale, x, y };
};

const getTransformString = (scale, x, y) =>
    "scale(" + scale + ") " + "translateX(" + x + "%) translateY(" + y + "%)";

const zoom = (direction) => {
    const { scale, x, y } = getTransformParameters(svg);
    let dScale = 0.1;
    if (direction == "out") dScale *= -1;
    if (scale == 0.1 && direction == "out") dScale = 0;
    svg.style.transform = getTransformString(scale + dScale, x, y);
};

document.getElementById("zoom-in-button").onclick = () => zoom("in");
document.getElementById("zoom-out-button").onclick = () => zoom("out");

function clearClass(className) {
    const element = document.getElementsByClassName(className)[0];
    if (element)
        element.classList.remove(className);
}

function setBackground(value) {
    workspace.style.backgroundColor = value;
}

function clearSvg() {
    if (!confirm("Are you sure you want to clear the workspace?"))
        return;
    const element = document.getElementById("workspace");
    removeChildren(element);
    updateAreaDetails(0, 0);
    selectedTool = null;
    currentAction = null;
    currentElement = null;
    currentBezierGroup = null;
    clearClass("activeShape");
    clearClass("activeTool");
    setBackground("");
}


function openGeneral() {

    var row = document.getElementById('row-shapes');
    if (row.style.display === "none") {
        row.style.display = "block";
    } else {
        row.style.display = "none";
    }
}

function openBezier() {

    var row = document.getElementById('row-bezier');
    if (row.style.display === "none") {
        row.style.display = "block";
    } else {
        row.style.display = "none";
    }
}

function openOthers() {

    var row = document.getElementById('row-others');
    if (row.style.display === "none") {
        row.style.display = "block";
    } else {
        row.style.display = "none";
    }
}

function openFilters() {

    var row = document.getElementById('row-filters');
    if (row.style.display === "none") {
        row.style.display = "block";
    } else {
        row.style.display = "none";
    }
}


function filterImage(filter) {
    if (currentElement.tagName != "image") {
        return;
    }

    const canvas = document.createElement("canvas");
    const context = canvas.getContext("2d");

    const canvasWidth = currentElement.getAttribute("width");
    const canvasHeight = currentElement.getAttribute("height");

    canvas.width = canvasWidth;
    canvas.height = canvasHeight;

    context.drawImage(currentElement, 0, 0, canvasWidth, canvasHeight);

    const sourceImageData = context.getImageData(0, 0, canvasWidth, canvasHeight);
    const blankOutputImageData = context.createImageData(
        canvasWidth,
        canvasHeight
    );

    const outputImageData = applyFilter(
        sourceImageData,
        blankOutputImageData,
        filter
    );

    context.putImageData(outputImageData, 0, 0);

    currentElement.setAttribute("href", canvas.toDataURL());
}

function applyFilter(sourceImageData, outputImageData, filter) {
    switch (filter) {
        case "threshold":
            return applyThreshold(sourceImageData);
        case "sharpen":
            return applyConvolution(sourceImageData, outputImageData, [
                0,
                -1,
                0,
                -1,
                5,
                -1,
                0,
                -1,
                0
            ]);
        case "blur":
            return applyConvolution(sourceImageData, outputImageData, [
                1 / 16,
                2 / 16,
                1 / 16,
                2 / 16,
                4 / 16,
                2 / 16,
                1 / 16,
                2 / 16,
                1 / 16
            ]);
        case "median":
            return applyMedian(sourceImageData, outputImageData);
        case "greenArea":
            return detectGreenAreaPercent(sourceImageData, outputImageData);
    }
}

function applyThreshold(sourceImageData, threshold = 127) {
    const src = sourceImageData.data;

    for (let i = 0; i < src.length; i += 4) {
        const r = src[i];
        const g = src[i + 1];
        const b = src[i + 2];
        const v = (r + g + b) / 3 >= threshold ? 255 : 0;
        src[i] = src[i + 1] = src[i + 2] = v;
    }

    return sourceImageData;
}

function applyConvolution(sourceImageData, outputImageData, kernel) {
    const src = sourceImageData.data;
    const dst = outputImageData.data;

    const srcWidth = sourceImageData.width;
    const srcHeight = sourceImageData.height;

    const side = Math.round(Math.sqrt(kernel.length));
    const halfSide = Math.floor(side / 2);

    const w = srcWidth;
    const h = srcHeight;

    for (let y = 0; y < h; y++) {
        for (let x = 0; x < w; x++) {
            let r = 0,
                g = 0,
                b = 0,
                a = 0;

            for (let cy = 0; cy < side; cy++) {
                for (let cx = 0; cx < side; cx++) {
                    const scy = y + cy - halfSide;
                    const scx = x + cx - halfSide;

                    if (scy >= 0 && scy < srcHeight && scx >= 0 && scx < srcWidth) {
                        let srcOffset = (scy * srcWidth + scx) * 4;
                        let wt = kernel[cy * side + cx];
                        r += src[srcOffset] * wt;
                        g += src[srcOffset + 1] * wt;
                        b += src[srcOffset + 2] * wt;
                        a += src[srcOffset + 3] * wt;
                    }
                }
            }

            const dstOffset = (y * w + x) * 4;

            dst[dstOffset] = r;
            dst[dstOffset + 1] = g;
            dst[dstOffset + 2] = b;
            dst[dstOffset + 3] = a;
        }
    }
    return outputImageData;
}

function sortNumber(a, b) {
    return a - b;
}

function applyMedian(sourceImageData, outputImageData) {
    const src = sourceImageData.data;
    const dst = outputImageData.data;

    const srcWidth = sourceImageData.width;
    const srcHeight = sourceImageData.height;

    const side = 3;
    const halfSide = 1;

    const w = srcWidth;
    const h = srcHeight;

    for (let y = 0; y < h; y++) {
        for (let x = 0; x < w; x++) {
            const rArr = [];
            const gArr = [];
            const bArr = [];

            for (let cy = 0; cy < side; cy++) {
                for (let cx = 0; cx < side; cx++) {
                    const scy = y + cy - halfSide;
                    const scx = x + cx - halfSide;

                    if (scy >= 0 && scy < srcHeight && scx >= 0 && scx < srcWidth) {
                        let srcOffset = (scy * srcWidth + scx) * 4;
                        rArr.push(src[srcOffset]);
                        gArr.push(src[srcOffset + 1]);
                        bArr.push(src[srcOffset + 2]);
                    }
                }
            }

            const dstOffset = (y * w + x) * 4;

            rArr.sort(sortNumber);
            gArr.sort(sortNumber);
            bArr.sort(sortNumber);

            dst[dstOffset] = rArr[4];
            dst[dstOffset + 1] = gArr[4];
            dst[dstOffset + 2] = bArr[4];
            dst[dstOffset + 3] = src[dstOffset + 3];
        }
    }
    return outputImageData;
}

function createArray(length) {
    var arr = new Array(length || 0),
        i = length;

    if (arguments.length > 1) {
        var args = Array.prototype.slice.call(arguments, 1);
        while (i--) arr[length - 1 - i] = createArray.apply(this, args);
    }

    return arr;
}

let testCounter = 0;
let maxTestCounter = 0;
function processLabel(label, value) {
    testCounter++;
    if (linkedLabels[label] === 0) {
        labels[label] += value;
        return label;
    }


    linkedLabels[label] = processLabel(linkedLabels[label], labels[label] + value);
    processedLabels[label] = 1;
    return linkedLabels[label];
}

function compareLabels(a, b) {
    if (a === 0 || b === 0 || a === b)
        return false;

    if (a > b) {
        labels[b]++;

    }
}

let queue = null;
let currentLabel = null;
let binaryData = null;

function checkAndQueue(pixel) {
    if (binaryData[pixel.y][pixel.x] === 1 && labelingArray[pixel.y][pixel.x] === 0) {
        labelingArray[pixel.y][pixel.x] = currentLabel;
        labels[currentLabel]++;
        queue.push(pixel);
    }
}

function detectGreenAreaPercent(sourceImageData, outputImageData) {
    const src = sourceImageData.data;
    const dst = outputImageData.data;
    const srcWidth = sourceImageData.width;
    const srcHeight = sourceImageData.height;
    binaryData = createArray(srcHeight, srcWidth);
    labels = [];
    labels.push(0);
    labelingArray = createArray(srcHeight, srcWidth);
    processedLabels = [];
    processedLabels.push(0);
    linkedLabels = [];
    linkedLabels.push(0);

    for (let i = 0, y = 0; y < srcHeight; y++) {
        for (let x = 0; x < srcWidth; x++) {
            const r = src[i++];
            const g = src[i++];
            const b = src[i++];
            i++;
            if (g > 40) {
                isGreenMajority = (r < 0.8 * g && b < g) || (r < g && b < 0.8 * g);
                binaryData[y][x] = isGreenMajority ? 1 : 0;
            }
            else
                binaryData[y][x] = 0;
            labelingArray[y][x] = 0;
        }
    }

    currentLabel = 0;
    queue = [];

    for (let i = 0, y = 0; y < srcHeight; y++) {
        for (let x = 0; x < srcWidth; x++) {
            currentLabel++;
            labels.push(0);

            checkAndQueue({ x: x, y: y });

            while (queue.length > 0) {
                const pixel = queue.pop();
                if (pixel.y > 0)
                    checkAndQueue({ x: pixel.x, y: pixel.y - 1 });
                if (pixel.x < srcWidth - 1)
                    checkAndQueue({ x: pixel.x + 1, y: pixel.y });
                if (pixel.y < srcHeight - 1)
                    checkAndQueue({ x: pixel.x, y: pixel.y + 1 });
                if (pixel.x > 0)
                    checkAndQueue({ x: pixel.x - 1, y: pixel.y });
            }
        }
    }

    console.log("labels");
    console.log(labels);

    let maxLabel = 0;
    let maxCount = 0;

    for (let i = 1; i < labels.length; i++) {
        if (labels[i] > maxCount) {
            maxLabel = i;
            maxCount = labels[i];
        }
    }

    console.log(`maxLabel = ${maxLabel}, count = ${maxCount}`);

    let area = 0;
    let largestArea = 0;

    for (let i = 0, y = 0; y < srcHeight; y++) {
        for (let x = 0; x < srcWidth; x++) {

            let g = src[i + 1];
            if (labelingArray[y][x] === maxLabel) {
                g = src[i + 1] * 2;
                g = g > 255 ? 255 : g;
                area++;
                largestArea++;
            }
            else if (labelingArray[y][x] > 0) {
                g = src[i + 1] * 1.1;
                g = g > 255 ? 255 : g;
                area++;
            }
            else {
                g /= 2;
            }

            dst[i] = src[i] / 2;
            dst[i + 1] = g;
            dst[i + 2] = src[i + 2] / 2;
            dst[i + 3] = src[i + 3];
            i += 4;
        }
    }

    const size = srcHeight * srcWidth;
    const areaPercent = Math.round(area / size * 100);
    const largestAreaPercent = Math.round(largestArea / size * 100);


    updateAreaDetails(areaPercent, largestAreaPercent);
    currentElement.setAttribute("area-percent", areaPercent);
    currentElement.setAttribute("largest-area-percent", largestAreaPercent);

    console.log(binaryData);

    return outputImageData;
}

function updateAreaDetails(area, largest) {
    const areaOutput = document.getElementById("area-percent");
    const largestOutput = document.getElementById("largest-area-percent");
    if (area > 0) {
        areaOutput.innerText = area;
        largestOutput.innerText = largest;
    }
    else {
        areaOutput.innerText = "";
        largestOutput.innerText = "";
    }
}

function showDetails() {

    var row = document.getElementById('details');
    if (row.style.display === "none") {
        row.style.display = "block";
    } else {
        row.style.display = "none";
    }
}

function moveCurrentElementTo(where) {
    let element = null;
    if (currentBezierGroup)
        element = currentBezierGroup;
    else if (currentElement)
        element = currentElement;
    else
        return;

    if (where === tools.backward && element.previousElementSibling) {
        svg.insertBefore(element, element.previousElementSibling);
    }
    else if (where === tools.forward && element.nextElementSibling) {
        svg.insertBefore(element, element.nextElementSibling.nextElementSibling);
    }
    else if (where === tools.toBack && svg.firstChild) {
        svg.insertBefore(element, svg.firstChild);
    }
    else if (where === tools.toFront) {
        svg.insertBefore(element, null);
    }
}