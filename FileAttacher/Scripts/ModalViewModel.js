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

    self.fileAttachs = []; // modal fileAtts arr

    self.showModal = function () {

        self.modal.show(true);
        /* Bind FineUploader to modal view */
        $('#fine-uploader').fineUploader({
            debug: true,
            request: {
                endpoint: 'S3Web/UploadFile'
            }
        }).on('complete', function (event, id, name, responseJSON) {
            /*
            (event) // jQuery.event
            (id) // index in S3??
            (name) // lemur.jpg
            (responseJSON) // {S3FileName: "240eadf5-ec49-4d02-93ea-a3a5404f28f7", success: true}
            */
            if (responseJSON.success) {
                console.log(responseJSON);
                self.fileAttachs.push(
                    {
                        g: responseJSON.S3FileName,
                        MimeType : '', // set on server side on save
                        Filename: name,
                        Extension: name.slice(name.indexOf('.'), name.length)
                    });
            } else {
                console.log('There was an error with FineUploader');
            }
        });
    }

    self.onModalClose = function () {
        self.fileAttachs = [];
    }

    self.onModalAction = function () {

        var fArr = self.fileAttachs;
        var fID = viewModel.MainViewModel.currFolderId();

        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/FileAtt/SaveUploads",
            data: JSON.stringify({ centerIndex: careCenterID, ID: fID, FileAtts: fArr }), // file guid
            success: function () {
                fArr.forEach(function (file) {
                    viewModel.MainViewModel.files.push(file);
                });
            },
            error: function (data) {
                console.log(data);
            }
        });

        self.fileAttachs = []; // clear 
    }
}