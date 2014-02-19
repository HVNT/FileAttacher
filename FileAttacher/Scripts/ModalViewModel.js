/* File Attacher stuff + ViewModel */
function ModalViewModel() {
    
    var self = this;
    var careCenterID = "Center/99"; // HARDCODED Care CenterID ... will be in a user profile model??

    ko.bindingHandlers.bootstrapModal = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var props = valueAccessor(),
                vm = bindingContext.createChildContext(viewModel);

            ko.utils.extend(vm, props);

            vm.close = function () {
                vm.show(false);
                vm.onClose();
            };

            vm.action = function () {
                vm.show(false);
                vm.onAction();
            }

            ko.utils.toggleDomNodeCssClass(element, "modal hide fade", true);
            ko.renderTemplate("myModal", vm, null, element);

            var showHide = ko.computed(function () {
                $(element).modal(vm.show() ? 'show' : 'hide');
            });

            return {
                controlsDescendantBindings: true
            };
        }
    }

    self.modal = {
        header: ko.observable("add files"),
        closeLabel: "cancel",
        primaryLabel: "save",
        show: ko.observable(false),

        onClose: function () {
            self.onModalClose();
        },

        onAction: function () {
            self.onModalAction();
        }
    }

    self.showModal = function () {

        self.fileAttachs = ko.observableArray(); // modal fileAtts arr

        self.modal.show(true);
        /* Bind FineUploader to modal view */
        $('#fine-uploader').fineUploader({
            request: {
                endpoint: 'S3Web/UploadFile'
            }
        }).on('complete', function (event, id, name, responseJSON) {
            if (responseJSON.success) {
                // trust me i know this is ridiculous.. but it works so screw it for now
                var _file = {
                    g: responseJSON.S3FileName,
                    MimeType : '', // set on server side on save
                    Filename: name,
                    Extension: name.slice(name.indexOf('.'), name.length)
                }
                console.log(self.fileAttachs());
                if (self.fileAttachs().length < 1) {
                    self.fileAttachs.push(_file)
                }
                
                var alreadyPresent = false;
                self.fileAttachs().forEach(function (file) {
                    if (_file.g == file.g) {
                        alreadyPresent = true;
                        //break?
                    }
                });
                if (!alreadyPresent) {
                    self.fileAttachs.push(_file)
                }
            }
            else {
                console.log('There was an error with FineUploader');
            }
        });
    }

    self.onModalClose = function () {
        console.log("i just closed");
        self.fileAttachs.removeAll();
    }

    self.onModalAction = function () {

        console.log("i just actioned");
        var fID = viewModel.MainViewModel.currFolderId();

        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/FileAtt/SaveUploads",
            data: JSON.stringify({ centerIndex: careCenterID, ID: fID, FileAtts: self.fileAttachs() }), // file guid
            success: function () {

                self.fileAttachs().forEach(function (file) {
                    viewModel.MainViewModel.files.push(file);
                    viewModel.MainViewModel.dragDrop().go();
                });
                self.fileAttachs.removeAll(); // clear 
                console.log(self.fileAttachs());
            },
            error: function (data) {
                console.log(data);
                self.fileAttachs.removeAll(); // clear 
            }
        });

    }
}