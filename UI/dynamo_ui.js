var stage = new Kinetic.Stage({
        container: 'container',
        width: 1024,
        height: 768
      });

var nodes = [];	//a list to hold all of the nodes

var layer = new Kinetic.Layer();

for(var i=0; i<1024; i+=100)
{
	for(var j=0; j<768; j+=100)
	{
		var n1 = new dynNode(i, j, 100,80);
		nodes.push(n1);
	}
}

for (var i=0;i<nodes.length;i++)
{ 
	nodes[i].draw();
}

// you need to add the layer to the stage last
stage.add(layer);

function dynPort(parent){
	this.draw = function(){

	}
}
function dynConnector(startPort, endPort){
	this.draw = function(){

	}
}

// constructor
function dynNode(origin_x, origin_y, width, height){
	var x = origin_x;
	var y = origin_y;
	var width = width;
	var height = height;
	var isDragging = false;
	var rect = null;

	var group = new Kinetic.Group();

	// add the methods to the prototype so that all of the 
	// Foo instances can access the private static
	this.draw = function() {
		rect = new Kinetic.Rect({
        x: x,
        y: y,
        width: width,
        height: height,
        fill: 'green',
        stroke: 'black',
        strokeWidth: 4,
        draggable: true
      });

		group.add(rect);	//add the group
		layer.add(rect);	//before the layer
		
		rect.on('dragstart', function(){});
		rect.on('dragend', function(){});
	}
}

// http://js-bits.blogspot.com/2010/07/canvas-rounded-corner-rectangles.html
function roundRect(ctx, x, y, width, height, radius, fill, stroke) {
  if (typeof stroke == "undefined" ) {
    stroke = true;
  }
  if (typeof radius === "undefined") {
    radius = 5;
  }
  ctx.beginPath();
  ctx.moveTo(x + radius, y);
  ctx.lineTo(x + width - radius, y);
  ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
  ctx.lineTo(x + width, y + height - radius);
  ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
  ctx.lineTo(x + radius, y + height);
  ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
  ctx.lineTo(x, y + radius);
  ctx.quadraticCurveTo(x, y, x + radius, y);
  ctx.closePath();
  if (stroke) {
    ctx.stroke();
  }
  if (fill) {
    ctx.fill();
  }        
}


