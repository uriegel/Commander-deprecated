
function ConflictCollection(nodeFactoryId, items) {
    this.items = items;
    this.older = false;
    this.itemNodeFactory = $(nodeFactoryId)[0];
}

ConflictCollection.prototype.initialize = function(tableView) {
    this.tableView = tableView;
};

ConflictCollection.prototype.getItems = function() {
    this.tableView.onItemsChanged(true);
};

ConflictCollection.prototype.getItemsCount = function() {
    return this.items.conflictItems.length;
};

ConflictCollection.prototype.insertItem = function(itemIndex) {
    var item = this.items.conflictItems[itemIndex];
    switch (item.__type) {
        case "OperationFileItem:#":
            return this.insertFileItem(item);
    }
};

ConflictCollection.prototype.insertFileItem = function(file) {
    var $node = $(this.itemNodeFactory.cloneNode(true));
    $node.find(".image").attr("src", file.imageUrl);
    $node.find("#name").html(file.name);
    var $ss = $node.find("#sourcesize");
    var $ts = $node.find("#targetsize");
    $ss.html(this.numberFormat(file.sourceFileSize));
    $ts.html(this.numberFormat(file.targetFileSize));
    if (file.sourceFileSize == file.targetFileSize) {
        $ss.addClass("equal");
        $ts.addClass("equal");
    }
    var $st = $node.find("#sourceTime");
    var $tt = $node.find("#targetTime");
    $st.html(file.sourceDateTimeString);
    $tt.html(file.targetDateTimeString);
    if (file.sourceDateTime > file.targetDateTime)
       $st.addClass("newer");
    else if (file.sourceDateTime < file.targetDateTime) {
       $tt.addClass("older");
        this.older = true;
    }
    else {
       $st.addClass("equal");
       $tt.addClass("equal");
    }
    if (file.sourceVersion) {
        var $sv = $node.find("#sourceVersion");
        var $tv = $node.find("#targetVersion");
        $sv.html(file.sourceVersion);
        $tv.html(file.targetVersion);
        if (file.sourceVersion > file.targetVersion)
            $sv.addClass("newer");
        else if (file.sourceVersion < file.targetVersion) {
            $tv.addClass("older");
            this.older = true;
        }
        else {
            $sv.addClass("equal");
            $tv.addClass("equal");
        }
    }
    return $node;
};

ConflictCollection.prototype.refreshItem = function($item, index) {
};

ConflictCollection.prototype.numberFormat = function(number) {
    var strNumber = number+"";
    var thSep = '.';
    if(strNumber.length > 3) {
        var begriff = strNumber;
        strNumber = "";
        for (var j = 3; j < begriff.length ; j+=3) {
            var extract = begriff.slice(begriff.length - j, begriff.length - j + 3);
            strNumber = thSep + extract +  strNumber + "";
        }
        var strfirst = begriff.substr(0, (begriff.length % 3 == 0) ? 3 : (begriff.length % 3));
        strNumber = strfirst + strNumber;
    }
    return strNumber;
};

ConflictCollection.prototype.canBeSelected = function(itemIndex) {
    return false;
};

ConflictCollection.prototype.getNameOnly = function(name) {
    var pos = name.lastIndexOf('.');
    if (pos == -1)
        return name;
    return name.substring(0, pos);
};

ConflictCollection.prototype.getExtension = function(name) {
    var pos = name.lastIndexOf('.');
    if (pos == -1)
        return null;
    return name.substring(pos);
};

