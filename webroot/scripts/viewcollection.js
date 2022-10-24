function ViewCollection(nodeFactoryId, items) {
    this.items = items;
    this.itemNodeFactory = $(nodeFactoryId)[0];
}

ViewCollection.prototype.initialize = function(tableView) {
    this.tableView = tableView;
};

ViewCollection.prototype.getItems = function() {
    this.tableView.onItemsChanged(true);
};

ViewCollection.prototype.getItemsCount = function() {
    return this.items.length;
};

ViewCollection.prototype.insertItem = function(itemIndex) {
    var item = this.items[itemIndex];
    var $node = $(this.itemNodeFactory.cloneNode(true));
    $node.find("#left").html(item.items[0]);
    $node.find("#right").html(item.items[1]);
    if (itemIndex == 0)
        $node.addClass("selected");
    return $node;
};

ViewCollection.prototype.refreshItem = function($item, index) {
    if (this.items[index].selected) 
        $item.addClass("selected");
    else
        $item.removeClass("selected");
};

ViewCollection.prototype.canBeSelected = function(itemIndex) {
    return true;
};

ViewCollection.prototype.toggleSelection = function($item, itemIndex) {
    if (!this.canBeSelected(itemIndex))
        return;
    
    if (this.items[itemIndex].selected) 
        this.items[itemIndex].selected = false;
    else
        this.items[itemIndex].selected = true;
    this.refreshItem($item, itemIndex);
};

