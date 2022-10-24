
function ViewSwitcher(activeView, savedViews) {
    this.dialog = new Dialog(activeView, null);
    this.dialog.columns = [{item: "Links"}, {item: "Rechts"}];
    this.views = new Array;
    var self = this;
    savedViews.forEach(function(item){
        self.views.push({
            items: [self.getName(item.left.directory), self.getName(item.right.directory)],
            paths: [item.left.directory, item.right.directory],
            selected: false
        });
    });
    this.views.push({ items: ["Abbrechen", ""], selected: false});
    this.dialog.itemsCollection = new ViewCollection("#templates .viewitem", this.views);
}
                    
ViewSwitcher.prototype.show = function() {
    this.dialog.show();
    this.dialog.tableView.toggleSelection();
};

ViewSwitcher.prototype.close = function() {
    this.dialog.close();
};

ViewSwitcher.prototype.getSelectedItem = function() {
    var result;
    this.views.forEach(function(item) {
        if (item.selected)
            result = item.paths;
    });
    return result;
};

ViewSwitcher.prototype.switch = function() {
    this.dialog.tableView.toggleSelection();
    if (!this.dialog.tableView.downOne())
        this.dialog.tableView.pos1();
    this.dialog.tableView.toggleSelection();
};

ViewSwitcher.prototype.getName = function(name) {
    var pos = name.lastIndexOf('\\');
    if (pos == -1)
        return name;
    
    var subpath = name.substring(0, pos);
    var pos2 = subpath.lastIndexOf('\\');
    if (pos2 == -1) {
        if (pos +1 == name.length)
            return name;
        else
            return name.substring(pos + 1);
    }
    
    pos = pos2;
    subpath = subpath.substring(0, pos);
    pos2 = subpath.lastIndexOf('\\');
    if (pos2 == -1)
        return name.substring(pos + 1);
    return name.substring(pos2 + 1);
};
