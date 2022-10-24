
function Scrollbar(parentId, positionChanged) {
    this.$parent = $("#" + parentId);
    this.$scrollbar = $(document.createElement("div"));
    this.$scrollbar.addClass("scrollbar");
    this.$scrollbar.hide();
    this.position = 0;
    this.positionChanged = positionChanged;
    this.gripTopDelta = -1;
    this.gripping = false;
    this.parentHeight;
    this.offsetTop = 0;
    
    this.$scrollbar.css("height", "100%");
    
    var $up = $(document.createElement("div"));
    $up.addClass("scrollbarUp");
    this.$scrollbar.append($up);

    var $upImg = $(document.createElement("div"));
    $upImg.addClass("scrollbarUpImg");
    $up.append($upImg);
    
    var $down = $(document.createElement("div"));
    $down.addClass("scrollbarDown");
    this.$scrollbar.append($down);

    var $downImg = $(document.createElement("div"));
    $downImg.addClass("scrollbarDownImg");
    $down.append($downImg);

    this.$grip = $(document.createElement("div"));
    this.$grip.addClass("scrollbarGrip");
    this.$scrollbar.append(this.$grip);

    this.$parent.append(this.$scrollbar);
    
    var self = this;
    
    $up.mousedown(function() {
        clearTimeout(self.timer);
        clearInterval(self.interval);
        self.mouseUp();

        self.timer = setTimeout(function() {
            self.interval = setInterval(function() {
                self.mouseUp();
            }, 20);
        }, 600);
    });

    $down.mousedown(function() {
        clearTimeout(self.timer);
        clearInterval(self.interval);
        self.mouseDown();
        
        self.timer = setTimeout(function() {
            self.interval = setInterval(function() {
                self.mouseDown();
            }, 20);
        }, 600);
    });
    
    this.$scrollbar.mousedown(function(evt) {
        if (!$(evt.target).hasClass("scrollbar"))
            return;
        
        self.pageMousePosition = evt.pageY;
        var pageUp = evt.pageY < self.$grip.offset().top;
       
        clearTimeout(self.timer);
        clearInterval(self.interval);
        if (pageUp)
            self.pageUp();
        else
            self.pageDown();

        self.timer = setTimeout(function() {
            self.interval = setInterval(function() {
                if (pageUp)
                    self.pageUp();
                else
                    self.pageDown();
            }, 20);
        }, 600);
    });
    
    this.$grip.mousedown(function(evt) {
        if (evt.which != 1)
            return;
        self.gripping = true;
        self.gripTopDelta = self.$grip.offset().top - evt.pageY ;
    });

    $(window).mousemove(function(evt) {
        if (!self.gripping || evt.which != 1)
            return;
        
        var top = evt.pageY - self.offsetTop + self.gripTopDelta - 15;
        if (top < 15)
            top = 15;
        else if (top + self.$grip.height() - 15 > (self.parentHeight - 32))
            top = self.parentHeight - 32 - self.$grip.height() + 15;
        self.$grip.css({ top: top + 'px' });
        
        var position = Math.floor((top - 15) / self.step + 0.5);
        if (position != self.position) {
            self.position = position;
            self.positionChanged(self.position);
        }
    });

    $(window).mouseup(function() {
        clearTimeout(self.timer);
        clearInterval(self.interval);
        self.gripping = false;
    });
    
    this.$scrollbar.mouseleave(function() {
        clearTimeout(self.timer);
        clearInterval(self.interval);
    });
};

Scrollbar.prototype.setPosition = function(newPosition) {
    this.position = newPosition;
    if (this.position > this.steps) 
        this.position = this.steps;
    if (this.position < 0) 
        this.position = 0;
    this.positionGrip();
};
    

Scrollbar.prototype.mouseDown = function() {
    this.position += 1;
    if (this.position > this.steps) {
        this.position = this.steps;
        clearTimeout(this.timer);
        clearInterval(this.interval);
        return;
    }
    this.positionGrip();
    this.positionChanged(this.position);
};

Scrollbar.prototype.mouseUp = function() {
    this.position -= 1;
    if (this.position < 0) {
        this.position = 0;
        clearTimeout(this.timer);
        clearInterval(this.interval);
        return;
    }
        
    this.positionGrip();
    this.positionChanged(this.position);    
};

Scrollbar.prototype.pageUp = function() {
    if (this.$grip.offset().top < this.pageMousePosition) {
        clearTimeout(this.timer);
        clearInterval(this.interval);
        return;
    }        

    this.position -= this.itemsCountVisible - 1;
    if (this.position < 0) {
        var lastTime = this.position != 0;
        this.position = 0;
        clearTimeout(this.timer);
        clearInterval(this.interval);
        if (lastTime) {
            this.positionGrip();
            this.positionChanged(this.position);
        }
        return;
    }
    this.positionGrip();
    this.positionChanged(this.position);
};

Scrollbar.prototype.pageDown = function() {
    if (this.$grip.offset().top + this.$grip.height() > this.pageMousePosition) {
        clearTimeout(this.timer);
        clearInterval(this.interval);
        return;
    }        

    this.position += this.itemsCountVisible - 1;
    if (this.position > this.steps) {
        var lastTime = this.position != 0;
        this.position = this.steps;
        clearTimeout(this.timer);
        clearInterval(this.interval);
        if (lastTime) {
            this.positionGrip();
            this.positionChanged(this.position);
        }
        return;
    }
    
    this.positionGrip();
    this.positionChanged(this.position);
};

Scrollbar.prototype.positionGrip = function() {
    var top = 15 + this.position * this.step;
    this.$grip.css({ top: top + 'px' });
};

Scrollbar.prototype.setOffsetTop = function(top) {
    this.offsetTop = top;
    this.$scrollbar.css("height", "calc(100% - " + top + "px");
    this.$scrollbar.css("top", top + "px");
};

Scrollbar.prototype.resize = function(height, itemsCountAbsolute, itemsCountVisible, newPosition) {
    this.parentHeight = height;
    if (itemsCountAbsolute)
        this.itemsCountAbsolute = itemsCountAbsolute;
    if (itemsCountVisible)
        this.itemsCountVisible = itemsCountVisible;
    
    if (!this.itemsCountAbsolute)
        return;
    
    if (this.itemsCountAbsolute <= this.itemsCountVisible) {
        this.$scrollbar.hide("slide");
    } else  {
        this.$scrollbar.show("slide");
        this.gripHeight = (height - 32) * (this.itemsCountVisible / this.itemsCountAbsolute);
        if (this.gripHeight < 5)
            this.gripHeight = 5;
        this.steps = this.itemsCountAbsolute - this.itemsCountVisible;
        this.step = (height - 32 - this.gripHeight) / this.steps;
        this.$grip.height(this.gripHeight);
        if (this.position > this.steps) 
            this.position = this.steps;
    }
    if (newPosition != undefined)
        this.position = newPosition;
    this.positionGrip();
};