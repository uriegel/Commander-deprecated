

function TableView(id, itemsCollection) {
    this.id = id;
    this.tableSelector = "#" + id + " tbody";
    this.itemsCollection = itemsCollection;
    this.itemsCollection.initialize(this);
    
    this.$tableView = $('#' + id);
    this.$tableView.css("position", "absolute");
    this.$tableView.css("clip", "rect(auto, auto, auto, auto");
    
    this.$tableView.css("width", this.$tableView.data("width"));
    this.$tableView.css("height", this.$tableView.data("height"));
    this.$tableView.css("outline", "none");
    
    this.scrollbar = new Scrollbar(id, function(position) {
        self.scroll(position);
    });
    
    this.onKeydown = null;
    
    var $tr = $("#" + id + " tbody tr");
    $tr.css("border-top-color", "transparent");
    $tr.css("border-top-style", "solid");
    $tr.css("border-top-width", "1px");
    $tr.css("border-left-color", "transparent");
    $tr.css("border-left-style", "solid");
    $tr.css("border-left-width", "1px");
    
    /**
     * Index des aktuellen Eintrags in der Liste der Einträge (items)
     * 
     * @type int
     */
    this.currentItem = 0;

    this.tableCapacity = -1;

    this.$table = $("<table></table>");
    this.$table.css("width", "100%");
    this.$table.css("table-layout", "fixed");
    
    this.$table.appendTo(this.$tableView);
    this.$thead = $("<thead></thead>");
    this.$thead.appendTo(this.$table);
    this.$tbody = $("<tbody></tbody>");
    this.$tbody.appendTo(this.$table);
    var $theadrow = $("<tr></tr>");
    $theadrow.appendTo(this.$thead);

    this.$table.css("border-collapse", "collapse");
    $tr = $('tr');
    $tr.css("-moz-user-select", "none");
    $tr.css("-webkit-user-select", "none");
    $tr.css("-ms-user-select", "none");

    $tr.css("overflow", "hidden");
    $tr.css("text-overflow", "ellipsis");
    $tr.css("white-space", "nowrap");

    var self = this;

    this.$tbody.click(function(evt) {
       var $tr = $(evt.target).closest("tr");
       self.onClick($tr.index());
       $tr.focus();
    });
    
    this.$tbody.dblclick(function() {
        self.onSelectedCallback(self.currentItem);
    });

    this.$tbody.bind('mousewheel', function(e) {
        var delta = e.originalEvent.wheelDelta / Math.abs(e.originalEvent.wheelDelta) * 3;
        self.scroll(self.startPosition - delta);
    });

    this.$tableView.keydown(function(e) {
        switch(e.which) {
            case 33: 
                self.pageUp();
                break;
            case 34: 
                self.pageDown();
                break;
            case 38: 
                self.upOne();
                break;
            case 40: 
                self.downOne();
                break;
            case 36:
                if (!e.shiftKey)
                    self.pos1();
                else if (self.onKeydown) {
                    self.onKeydown(e);
                    return;
                }
                break;
            case 35:
                if (!e.shiftKey)
                    self.end();
                else if (self.onKeydown) {
                    self.onKeydown(e);
                    return;
                }
                break;
            case 13:
                self.onSelectedCallback(self.currentItem, e.ctrlKey);
                break;
             default:
                if (self.onKeydown)
                    self.onKeydown(e);
                return; // exit this handler for other keys
        }
        e.preventDefault(); // prevent the default action (scroll / move caret)
    });
}

TableView.prototype.setColumns = function(columnsControl) {
    this.columnsControl = columnsControl;
    
    var $theadrow = $('#' + this.id + ' thead tr');
    $theadrow.empty();
    this.columnsControl.columns.forEach(function(item){
        var $th = $('<th>' + item.item + '</th>');
        if (item.class)
            $th.addClass(item.class);
        $th.appendTo($theadrow);
    });
    this.columnsControl.initialize(this.id);
    
    // Vielleicht noch auf die TableView beschränken??
    var $tdth = $("td,th");
    $tdth.css("overflow", "hidden");
    $tdth.css("text-overflow", "ellipsis");
    $tdth.css("white-space", "nowrap");
};

TableView.prototype.resize = function(newWidth, newHeight) {
    this.$tableView.data("width", newWidth);
    this.$tableView.data("height", newHeight);
    this.$tableView.css("width", newWidth);
    this.$tableView.css("height", newHeight);
    this.onResize(newWidth, newHeight);
};

TableView.prototype.process = function(path, openWith) {
    if (this.onProcess)
        this.onProcess(path, openWith);
};

TableView.prototype.offset = function(offset) {
    this.$tableView.offset(offset);
};

TableView.prototype.focus = function() {
    $(this.tableSelector + ' tr:eq(' + (this.currentItem - this.startPosition) + ')').focus();        
};

TableView.prototype.getMaxTableHeight = function() {
    this.initializeRowHeight();
    return this.$thead.height() + this.itemsCollection.getItemsCount() * this.rowHeight;
};

TableView.prototype.getOffset = function() {
    var $table = $("#" + this.id + " table");
    return $table.offset();
};

TableView.prototype.onCurrentItemChanged = function(callback) {
    this.onCurrentItemChangedCallback = callback;
};

TableView.prototype.onItemsChanged = function(setFocus, lastCurrentItem, noCallback) {
    var start = 0;
    if (lastCurrentItem) {
        this.currentItem = lastCurrentItem;
        if (this.currentItem > this.tableCapacity)
            start = this.currentItem;
    }
    else
        this.currentItem = 0;

    if (this.onPreItemsChangedCallback)
        this.onPreItemsChangedCallback(this.itemsCollection.items.currentDirectory);

    if (!noCallback && this.onItemsChangedCallback)
        this.onItemsChangedCallback(this.itemsCollection.items.currentDirectory);

    this.displayItems(start);

    if (setFocus)
        $(this.tableSelector + ' tr:eq(' + (this.currentItem - start) + ')').focus();        
   
    if (this.onCurrentItemChangedCallback)
        this.onCurrentItemChangedCallback(this.currentItem);
    
};

TableView.prototype.displayItems = function(start) {
    this.startPosition = start;
    
    if (this.tableCapacity == -1) {
        this.initializeRowHeight();
        this.calculateTableHeight();
    }
    
    this.itemsCount = this.itemsCollection.getItemsCount();
    var end = Math.min(this.tableCapacity + 1 + start, this.itemsCount);
    for (var i = start; i < end; i++) {
       this.insertItem(i);
    }
    this.scrollbar.resize(this.$tableView.data("height") - this.$thead.height(), this.itemsCount, this.tableCapacity, this.startPosition);
};

TableView.prototype.insertItem = function(i, prepend) {
    var $node = this.itemsCollection.insertItem(i);
    if (prepend)
        $node.prependTo(this.tableSelector);
    else
        $node.appendTo(this.tableSelector);
};

TableView.prototype.scroll = function(position) {
    if (this.itemsCount < this.tableCapacity)
        return;
    if (position < 0)
        position = 0;
    else if (position > this.itemsCount - this.tableCapacity)
        position = this.itemsCount - this.tableCapacity;
    this.startPosition = position;
    this.clearItems();
    this.displayItems(this.startPosition);
    
    var selector = this.currentItem - this.startPosition;
    if (selector >= 0 && selector < this.tableCapacity)
        $(this.tableSelector + ' tr:eq(' + (this.currentItem - this.startPosition) + ')').focus();
    else {
        this.$tableView.focus();        
    }
};

TableView.prototype.scrollIntoView = function() {
    var selector = this.currentItem - this.startPosition;
    if (selector < 0) {
        this.scroll(this.currentItem);
    } else if (selector >= this.tableCapacity) {
        this.scroll(this.currentItem);
        this.scroll(this.currentItem - this.tableCapacity + 1);
    }
};
    
TableView.prototype.upOne = function() {
    if (this.currentItem == 0)
        return;
    
    this.scrollIntoView();
    
    this.currentItem--;
    if (this.currentItem - this.startPosition < 0) {
        if (this.currentItem + this.tableCapacity < this.itemsCount - 1)
            $(this.tableSelector + ' tr:last-child').remove();
        if (this.currentItem >= 0) {
            this.startPosition -= 1;
            this.scrollbar.setPosition(this.startPosition);
            this.insertItem(this.currentItem, true);      
        }
    }
        
    $(this.tableSelector + " tr:eq(" + (this.currentItem - this.startPosition) + ")").focus();
    if (this.onCurrentItemChangedCallback)
        this.onCurrentItemChangedCallback(this.currentItem);
};

TableView.prototype.downOne = function() {
    if (this.currentItem == this.itemsCount - 1)
        return false;
    
    this.scrollIntoView();

    this.currentItem++;
    if (this.currentItem - this.startPosition >= this.tableCapacity) {
        $(this.tableSelector + ' tr:first-child').remove();
        this.startPosition += 1;
        this.scrollbar.setPosition(this.startPosition);
        if (this.currentItem < this.itemsCount - 1)
            this.insertItem(this.currentItem + 1);    
    }
        
    $(this.tableSelector + " tr:eq(" + (this.currentItem - this.startPosition) + ")").focus();
    if (this.onCurrentItemChangedCallback)
        this.onCurrentItemChangedCallback(this.currentItem);
    return true;
};

TableView.prototype.pageUp = function() {
    if (this.currentItem == 0)
        return;

    this.scrollIntoView();

    if (this.currentItem - this.startPosition > 0) {
        this.currentItem = this.startPosition;
    }
    else {
        this.currentItem -= this.tableCapacity;
        if (this.currentItem < 0)
            this.currentItem = 0;
        this.clearItems();
        this.displayItems(this.currentItem);
    }
    
    $(this.tableSelector + " tr:eq(" + (this.currentItem - this.startPosition) + ")").focus();
    if (this.onCurrentItemChangedCallback)
        this.onCurrentItemChangedCallback(this.currentItem);
};

TableView.prototype.pageDown = function() {
    if (this.currentItem == this.itemsCount - 1)
        return;

    this.scrollIntoView();

    if (this.currentItem - this.startPosition < this.tableCapacity - 1) {
        this.currentItem = Math.min(this.tableCapacity, this.itemsCount) - 1 + this.startPosition;
        if (this.currentItem >= this.itemsCount)
            this.currentItem = this.itemsCount - 1;
    }
    else {
        this.currentItem += this.tableCapacity;
        if (this.currentItem >= this.itemsCount)
            this.currentItem = this.itemsCount - 1;
        this.clearItems();
        this.displayItems(this.currentItem - this.tableCapacity + 1);
    }

    $(this.tableSelector + " tr:eq(" + (this.currentItem - this.startPosition) + ")").focus();
    if (this.onCurrentItemChangedCallback)
        this.onCurrentItemChangedCallback(this.currentItem);
};

TableView.prototype.pos1 = function() {
    this.clearItems(); 
    this.displayItems(0);
    this.currentItem = 0;
    $(this.tableSelector + " tr:eq(0)").focus();
    if (this.onCurrentItemChangedCallback)
        this.onCurrentItemChangedCallback(this.currentItem);
};

TableView.prototype.end = function() {
    this.clearItems(); 
    this.currentItem = this.itemsCount - 1;
    var startPos = this.currentItem - this.tableCapacity + 1;
    if (startPos < 0)
        startPos = 0;
    this.displayItems(startPos);
    $(this.tableSelector + " tr:eq(" + (this.currentItem - this.startPosition) + ")").focus();
    if (this.onCurrentItemChangedCallback)
        this.onCurrentItemChangedCallback(this.currentItem);
};

TableView.prototype.hasFocus = function() {
    return $(this.tableSelector + ' tr:eq(' + (this.currentItem - this.startPosition) + ')').is(":focus");
};

TableView.prototype.onResize = function(newWidth, newHeight) {
    if (this.columnsControl)
        this.columnsControl.resize(newWidth);
    
    var hasFocus = this.hasFocus();
        
    if (!hasFocus) 
        hasFocus = this.$tableView.is(":focus");

    if (!this.itemsCollection.items || this.itemsCount == 0) {
        return;
    }
    this.calculateTableHeight();

    clearInterval(this.resizeScrollUpTimerInterval);
    var self = this; 
    this.resizeScrollUpTimerInterval = setInterval(function() {
        clearInterval(self.resizeScrollUpTimerInterval);
        self.clearItems();
        var start = self.startPosition;
        if (self.itemsCount - start < self.tableCapacity)
            start = self.itemsCount - self.tableCapacity;
        if (start < 0)
            start = 0;
        self.displayItems(start);
        var diff = self.$table.height() - newHeight;
        if (diff > 0)
            self.$tableView.css("-webkit-clip-path", "inset(0px 0px 0px 0px)");
        
        self.setClipRegion(newWidth, newHeight);
        
        if (hasFocus) {
            var pos = self.currentItem - self.startPosition;
            if (pos < self.tableCapacity) 
                $(self.tableSelector + ' tr:eq(' + pos + ')').focus();        
            else { 
                self.$tableView.attr("tabIndex", "3");
                $('#' + self.tableView + ' :focus').css("outline", "none");
                self.$tableView.focus();        
            }
        }
    }, 50);
    
    this.scrollbar.resize(newHeight - this.$thead.height(), this.itemsCount, this.tableCapacity);

    this.setClipRegion(newWidth, newHeight);
};

TableView.prototype.onSelected = function(callback) {
    this.onSelectedCallback = callback;
};

TableView.prototype.clearItems = function() {
    this.$tbody.empty(); 
};

TableView.prototype.refresh = function(setFocus) {
    this.itemsCollection.refresh(setFocus);
};

TableView.prototype.refreshItems = function() {
    var self = this;
    $(this.tableSelector + ' tr').each(function(index){
        self.itemsCollection.refreshItem($(this), index + self.startPosition);
    });
};

TableView.prototype.initializeRowHeight = function() {
    var $node = this.itemsCollection.insertItem(0);
    $node.appendTo(this.tableSelector);
    this.rowHeight = $node.outerHeight();
    this.clearItems();
};

TableView.prototype.calculateTableHeight = function() {
    if (this.rowHeight)
        this.tableCapacity = Math.floor((this.$tableView.data("height") - this.$thead.height()) / this.rowHeight);
    else
        this.tableCapacity = -1;
    this.scrollbar.setOffsetTop(this.$thead.height());
    this.scrollbar.resize(this.$tableView.data("height") - this.$thead.height(), undefined, this.tableCapacity);
};

TableView.prototype.setClipRegion = function(width, height) {
    var path = "polygon(0px 0px, " + width + "px 0px, " + width + "px " + height + "px, 0px " + height + "px)";
    this.$tableView.css("-webkit-clip-path", path);    
};    

TableView.prototype.onClick = function(index) {
    this.currentItem = index + this.startPosition;
    if (this.onCurrentItemChangedCallback)
        this.onCurrentItemChangedCallback(this.currentItem);
};

TableView.prototype.toggleSelection = function() {
    var $item = $(this.tableSelector + ' tr:eq(' + (this.currentItem - this.startPosition) + ')');
    this.itemsCollection.toggleSelection($item, this.currentItem);
};

TableView.prototype.selectAll = function(select, startIndex) {
    this.itemsCollection.selectAll(select, startIndex);
    this.refreshItems();
};

TableView.prototype.getWidth = function() {
    return this.$tableView.outerWidth();
};

TableView.prototype.getCurrentItemIndex = function() {
    return this.currentItem;
};

