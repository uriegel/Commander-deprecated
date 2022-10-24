
function Dialog(elementToActivate, data) {
    this.text = null;
    this.data = data;
    this.elementToActivate = elementToActivate;
    this.tableView = null;
    this.itemsCollection = null;
    this.buttonBar = false;
    this.columns = null;
}

Dialog.prototype.show = function(callback) {
    var $shader = $("#shader");
    $shader.stop();
    $shader.show();
    $shader.animate({opacity: 0.5}, 300);
    $('body').append("<div class='dialog'></div>");
    var $dialog = $("body .dialog");
    var $dialogOk;
    var $dialogCancel;
    if (this.text)
        $dialog.append('<div id="text">' + this.text +'</div><p></p>');
    if (this.itemsCollection) {
        $dialog.append('<div id="tableView"></div>');
        $dialog.append('<div id="tableViewFlow"></div>');
        this.tableView = new TableView("tableView", this.itemsCollection);
        var columns = new ColumnsControl(this.columns);
        this.tableView.setColumns(columns);
        
        if (this.buttonBar) {
            $dialog.append("<p></p><div id=buttonBar><input id='ok' type='button' value='Ja'><input id='cancel' type='button' value='Nein'></div>");
            $dialogOk = $(".dialog #ok");
            $dialogCancel = $(".dialog #cancel");
            $dialogOk.css("margin-right", "20px");
            $dialogCancel.css("width", "60px");
            $dialogOk.keydown(function(e) {
                if (e.which == 9 || e.which == 39) {
                    $(".dialog #cancel").focus();
                    e.preventDefault();
                }
            });
            $dialogCancel.keydown(function(e) {
                if (e.which == 9 || e.which == 37) {
                    $(".dialog #ok").focus();
                    e.preventDefault();
                }
            });
        }
    }
    else
        $dialog.append("<p></p><div id=buttonBar><input id='ok' type='button' value='OK'></div>");
        $dialogOk = $(".dialog #ok");
        $(".dialog").keydown(function(e) {
            if (e.which == 27) {
                self.close();
            }
        });
    $dialogOk.css("width", "60px");
    var $buttonBar = $(".dialog #buttonBar");
    $buttonBar.css("position", "relative");
    $buttonBar.css("padding-bottom", "10px");
    $buttonBar.css("text-align", "right");
    
    var self = this;
    $dialogOk.click(function() {
        self.close();
        if (callback)
            callback(true);
    });
    if ($dialogCancel)
        $dialogCancel.click(function() {
            self.close();
            if (callback)
                callback(false);
        });

    $(window).resize({self: this}, dialogResize);

    this.$focusButton = $dialogOk;
    dialogResize({data: {self: self}});
    if (this.tableView) {
        this.tableView.itemsCollection.getItems();   
        if (this.tableView.itemsCollection.older)
            this.$focusButton = $(".dialog #cancel");
    }

    dialogResize({data: {self: self}});
    
    this.$focusButton.focus();
};

Dialog.prototype.showButtonBar = function() {
    this.buttonBar = true;
};

Dialog.prototype.close = function() {
    $("#shader").animate({opacity: 0}, 800, function() {
        $("#shader").hide();
    });

    var self = this;
    $(".dialog").animate({opacity: 0}, 350, function() {
        $(".dialog").remove();
        $(window).off("resize", dialogResize);
        self.elementToActivate.focus();
    });
};

function dialogResize(evt) {
    var self = evt.data.self;
    var $dialog = $(".dialog");
    var windowWidth = $(window).width();
    var windowHeight = $(window).height();
    var w = $dialog.outerWidth();
    var h = $dialog.outerHeight();
    var top = (windowHeight - h) / 2;
    var left = (windowWidth - w) / 2;
    if (self.tableView) {
        var $buttonBar = self.buttonBar ? $(".dialog #buttonBar") : null;
        var $tableViewFlow = $(".dialog #tableViewFlow");
        var buttonBarHeight = self.buttonBar ? $buttonBar.height() : 0;
        var tableOffset = self.tableView.getOffset().top;
        var dialogOffset = $dialog.offset().top;
        if (!self.tableHeight)
            self.tableHeight = self.tableView.getMaxTableHeight();
        var height = self.tableHeight + tableOffset - dialogOffset + buttonBarHeight;
        if (!height)
            height = 200000;
        var newWidth = Math.min(windowWidth - 120, 1000);
        var newHeight = Math.min(windowHeight - 120, height) + 3;
        self.tableView.resize(newWidth, newHeight - tableOffset + dialogOffset - buttonBarHeight);
        $tableViewFlow.width(newWidth);
        $tableViewFlow.height(newHeight - tableOffset + dialogOffset - buttonBarHeight);
        $dialog.width(newWidth);
        $dialog.height(newHeight);
        top = (windowHeight - newHeight - 40) / 2;
        left = (windowWidth - newWidth - 40) / 2;
    }
    $dialog.offset({left: left, top: top});
    if (self.$focusButton) {
        self.$focusButton.focus();    
        setTimeout(function() {
            self.$focusButton.focus();    
        }, 50);
    }
}
