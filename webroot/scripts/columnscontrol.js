function ColumnsControl(columns) {
    this.columns = columns;
    this.grippingReady = false;
    this.isGripping = false;
    this.startGripPosition = 0;
    this.currentLeftWidth = 0;
    this.sumWidth = 0;
    this.previous = false;
    this.$currentHeader = null;
    this.$nextHeader = null;
    this.resizeTimer;
}

ColumnsControl.prototype.restoreSettings = function(settings, tableViewWidth) {
    if (settings) {
        this.setWidths(settings, tableViewWidth);
    }
    else {
        var columnLength = tableViewWidth / this.columns.length;
        var widths = new Array;
        for (i = 0; i < this.columns.length; i++) {
            widths.push(columnLength);
        }
        this.setWidths(widths, tableViewWidth);
    }
};

ColumnsControl.prototype.getSettings = function() {
    return this.getWidths();
};

ColumnsControl.prototype.initialize = function(id) {
    this.headSelector = "#" + id + " th";

    var self = this;

    $(self.headSelector).mousedown(function(evt) {
        if (!self.grippingReady)
            return;
        self.gripStarted = true;
        $('html,body,thead').css('cursor','ew-resize');
        self.startGripPosition = evt.pageX;

        self.isGripping = true;
        self.currentWidth = 0;
        if (!self.previous)
            self.$currentHeader = $(this);
        else
            self.$currentHeader = $(this).prev();
        self.$nextHeader = self.$currentHeader.next();
        self.currentLeftWidth = self.$currentHeader.outerWidth();
        self.sumWidth = self.currentLeftWidth + self.$nextHeader.outerWidth();
        evt.preventDefault(); 
    });
                
    $("#" + id).mousemove(function(evt) {
        if (!self.isGripping) {
            var $th = $(evt.target);
            if ($th.is("th") && ($th.offset().left + $th.outerWidth() - evt.pageX < 4 || evt.pageX - $th.offset().left < 4)) {
                $('html,body,thead').css('cursor','ew-resize');
                self.grippingReady = true;
                self.previous = evt.pageX - $th.offset().left < 4;
            }
            else {
                $('html,body,thead').css('cursor','default');
                self.grippingReady = false;
            }
        }
        else {
            if (evt.which == 0) {
                self.isGripping = false;
                $('html,body,thead').css('cursor','default');
                return;
            }

            var diff = evt.pageX - self.startGripPosition;

            if (self.currentLeftWidth + diff < 15)
                diff = 15 - self.currentLeftWidth;
            else if (diff > self.sumWidth - self.currentLeftWidth - 15)
                diff = self.sumWidth - self.currentLeftWidth - 15;

            self.$currentHeader.outerWidth(self.currentLeftWidth + diff);
            self.$nextHeader.outerWidth(self.sumWidth - self.currentLeftWidth - diff);

            evt.preventDefault(); 
        }
    });        
    
    $("#" + id).mouseup(function(evt) {
        if (self.isGripping) {
            self.isGripping = false;
            $('html,body,thead').css('cursor','default');
            return;
        }
    });

    $("#" + id).mouseleave(function(evt) {
        if (self.isGripping) {
            self.isGripping = false;
            $('html,body,thead').css('cursor','default');
            return;
        }
    });
};

ColumnsControl.prototype.getWidths = function() {
    var widths = new Array();
    $(this.headSelector).each(function(i){
        widths[i] = $(this).outerWidth();
    });
    return widths;
};
    
ColumnsControl.prototype.setWidths = function(widths, newWidth) {
    var sumWidth = 0;
    widths.forEach(function(item){
        sumWidth += item;
    });

    var factor = newWidth / sumWidth;

    var headLeft;
    $(this.headSelector).each(function(i){
        var width = widths[i] * factor;
        if (i== 0)
            headLeft = $(this).offset().left;
        if (i == widths.length - 1) {
            width += 20;
            $(this).outerWidth(width);
            var left = $(this).offset().left;
            width = newWidth + headLeft - left;
        }
        $(this).outerWidth(width);
    });
};

ColumnsControl.prototype.resize = function(newWidth) {
    var self = this;
    clearInterval(this.resizeTimer);
    this.resizeTimer = setInterval(function() {
        clearInterval(self.resizeTimer);

        var widths = new Array();
        $(self.headSelector).each(function(i){
            widths[i] = $(this).outerWidth();
        });

        var sumWidth = 0;
        widths.forEach(function(item){
            sumWidth += item;
        });

        var factor = newWidth / sumWidth;

        var headLeft;
        $(self.headSelector).each(function(i){
            var width = widths[i] * factor;
            if (i== 0)
                headLeft = $(this).offset().left;
            if (i == widths.length - 1) {
                width += 20;
                $(this).outerWidth(width);
                var left = $(this).offset().left;
                width = newWidth + headLeft - left;
            }
            $(this).outerWidth(width);
        });
    }, 100);
};
