const shapeBtns = document.querySelectorAll(".shape"),
    toolBtns = document.querySelectorAll(".tool"),
    fillColor = document.querySelector("#fill-color"),
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
    scaleBezier: "scale-bezier"
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

sizeSlider.addEventListener("change", () => brushWidth = sizeSlider.value);

const changeColor = document.querySelector("#changeColor");

changeColor.addEventListener("change", () => brushColor = changeColor.value);

let brushColor = "black";

function setSelectedShape(shape) {
    selectedShape = shape;
}

function setSelectedTool(tool) {
    switch (selectedTool) {
        case tools.drawBezier:
        case tools.modifyBezier:
        case tools.moveBezier:
        case tools.scaleBezier:
            break;
        default:
            currentBezierGroup = null;
            currentBezierPoints = null;
            break;
    }
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
    currentElement.setAttribute("fill", "none");
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
            break;
        }
    }
    currentElement.setAttribute("cx", x);
    currentElement.setAttribute("cy", y);
    currentBezierPoints[id].x = x;
    currentBezierPoints[id].y = y;
    generateBezierCurve();
}

function createInput(value) {
    const input = document.createElement("input");
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
    const newControl = document.createElement("div")
    newControl.setAttribute("point-id", id);
    newControl.appendChild(inputX);
    newControl.appendChild(inputY);
    newControl.appendChild(button);
    const box = document.getElementById("bezier-points");
    box.appendChild(newControl);
}

function addPointToBezier(x, y) {
    const point = document.createElementNS(svgns, "circle");
    point.setAttribute("cx", x);
    point.setAttribute("cy", y);
    point.setAttribute("r", 5);
    point.setAttribute("fill", "red");
    const id = currentBezierPoints.length;
    point.setAttribute("point-id", id);
    currentBezierGroup.appendChild(point);
    currentBezierPoints.push({ x: x, y: y });
    createBezierPointControl(x, y, id);
}

function startDrawBezier(startX, startY) {
    currentBezierGroup = document.createElementNS(svgns, "g");
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
    moving(dY, dY);
    currentBezierPoints[id].x += dX;
    currentBezierPoints[id].y += dY;
    generateBezierCurve();
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

function onObjectMouseDown(e) {
    if (selectedTool === tools.draw) {
        return;
    }

    e.stopPropagation();
    currentElement = e.target;

    if (currentElement.parentElement.tagName === "g") {
        currentBezierGroup = currentElement.parentElement;
    }

    currentAction = selectedTool;

    if (currentElement.tagName === "polygon") {
        setCurrentPolygonAttributesFromCurrentElement();
    }

    if (currentAction === tools.scale)
        switch (currentElement.tagName) {
            case "rect":
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

function onSvgMouseDown(e) {
    if (selectedTool === tools.draw && selectedShape) {
        startDraw(e);
    }
    else if (selectedTool === tools.drawBezier) {
        startDrawBezier(e.offsetX, e.offsetY);
    }
    else if (selectedTool === tools.pencil) {
        currentAction = tools.pencil;
        path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        path.setAttribute("fill", "none");
        path.setAttribute("stroke", brushColor);
        path.setAttribute("stroke-width", brushWidth);
        buffer = [];
        let pt = getMousePosition(e);
        appendToBuffer(pt);
        strPath = "M" + e.offsetX + " " + e.offsetY;
        //strPath = "M" + pt.x + " " + pt.y;
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
}

function onMouseDown(e) {
    if (selectedTool === tools.drawPolygon) {
        drawPolygonPoint(e.offsetX, e.offsetY);
    }
    else if (selectedTool === tools.drawBezier && currentBezierGroup) {
        drawBezierPoint(e.offsetX, e.offsetY);
    }
    else if (selectedTool === tools.scale && currentElement.tagName === "g" && e.target.tagName === "circle") {
        startScaleCurve(e);
    }
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
    }
}

svg.addEventListener("mousedown", onMouseDown);
svg.addEventListener("mousemove", onMouseMove);
svg.addEventListener("mouseup", onMouseUp);


// pencil

var getMousePosition = function (e) {
    return {
        x: e.pageX - boundingRect.left,
        y: e.pageY - boundingRect.top
    }
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
        };
        reader.readAsText(file);
    };
    input.click();
}

function saveSvg() {
    svg.setAttribute("xmlns", "http://www.w3.org/2000/svg");
    var svgData = svg.outerHTML;
    var preface = '<?xml version="1.0" standalone="no"?>\r\n';
    var svgBlob = new Blob([preface, svgData], { type: "image/svg+xml;charset=utf-8" });
    var svgUrl = URL.createObjectURL(svgBlob);
    var downloadLink = document.createElement("a");
    downloadLink.href = svgUrl;
    downloadLink.download = "example";
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

function clearSvg() {
    const element = document.getElementById("workspace");
    element.innerHTML = "";
}
