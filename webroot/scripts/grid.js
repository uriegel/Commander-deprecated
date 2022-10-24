
function Grid(id) {
    this.id = id;
    this.$grip = $('#' + id);
    this.$grip.css("position", "absolute");
    this.$grip.css("cursor", "ew-resize");
    this.gripWidth = this.$grip.width();
    this.gripPosition;
    this.gridWidth;
    this.gridHeight;
    
    this.factor = 0.5;
    
    this.gripStarted = false;
    
    var self = this;
    this.$grip.mousedown(function(evt){
        self.gripStarted = true;
        $('html,body').css('cursor','ew-resize');
        self.gripPosition = evt.pageX;
        evt.preventDefault(); 
    });
    
    $(window).mousemove(function(evt){
        if (self.gripStarted) {
            if (evt.which == 0){
                self.gripStarted = false;
                $('html,body').css('cursor','default');
                return;
            }

            if (self.factor == 0.98) {
                if (self.$grip.offset().left < evt.pageX)
                    return;
            }
            else if (self.factor == 0.02) {
                if (self.$grip.offset().left > evt.pageX)
                    return;
            }

            var newGripPosition = evt.pageX;
            var diff = newGripPosition - self.gripPosition;

            var newGrip = self.gridWidth * self.factor + diff;
            self.factor = newGrip / self.gridWidth;
            
            if (self.factor > 0.98)
                self.factor = 0.98;
            if (self.factor < 0.02)
                self.factor = 0.02;

            self.gripPosition = newGripPosition;
            self.resize();
            evt.preventDefault(); 
        }
    });
    
    $(window).mouseup(function(){
        if (self.gripStarted) {
            self.gripStarted = false;
            $('html,body').css('cursor','default');
        }
    });
};

Grid.prototype.setLeft = function(item) {
    this.left = item;
    this.left.offset({ top: 0, left: 0 });    
};

Grid.prototype.setRight = function(item) {
    this.right = item;
};

Grid.prototype.resize = function(width, height) {
    if (width)
        this.gridWidth = width;
    if (height)
        this.gridHeight = height;
    var middle = Math.floor(this.gridWidth * this.factor);
    var gripHalf = Math.floor(this.gripWidth / 2);
    var gripHalfRight = this.gripWidth - gripHalf;
    this.$grip.offset({ top: 0, left: middle - gripHalf });
    this.$grip.height(this.gridHeight);
    this.left.resize(middle - gripHalf, this.gridHeight);
    this.right.offset({ top: 0, left: middle + gripHalfRight });    
    this.right.resize(this.gridWidth -  middle - gripHalfRight - 1, this.gridHeight);
};


