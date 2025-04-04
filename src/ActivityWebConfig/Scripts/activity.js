var jsonEditor = null;          //Keeps the reference to the JSON editor control
var jsonEditorDialog = null;    //Keeps the reference to the JSON editor control in platform provided data dialog
var bridge = null;              //Keeps the reference to the script that serves as a bridge between platform and activity
var activitySettings;           //JSON object with activity configuration. This object is given by the platform, and must be returned to the platform
var size = 0;                   //Size of the uploaded binary file 
var dialog = null;              //Reference to the variable dialog
var jsonDialog = null;          //Reference to the platform provided data JSON dialog
var dialogOwner = null;         //Reference to the control that raised the variable dialog
var dialogDocType = null;       //Reference to the control that raised the document type dialog
var dialogField = null;         //Reference to the control that raised the index field dialog
var dialogTable = null;         //Reference to the control that raised the table dialog
var dialogColumn = null;        //Reference to the control that raised the column dialog
var form = null;                //Reference to the form in a dialog
var formfields = null;          //Reference to the form's fields in a dialog

//This script shows how to generate UI based on the data from the platform; how to manipulate variables and how to return modified data.
//A basic knowledge of JQuery and JQuery UI is required
$(function () {
	//Document ready function; fired when DOM is ready; here set up the activity
	$(document).ready(function () {
		//UI stuff
        //Enable localization. More info here: https://github.com/coderifous/jquery-localize
        doLocalization();
		//Enable tabs widget. More info here:https://jqueryui.com/tabs/
		$("#tabs").tabs();
		//Enable tool-tip widget. More info here: https://jqueryui.com/tooltip/
        $("#tabs").tooltip();
        //Enable menu widget. More info here: https://jqueryui.com/menu/
        var menu = $("#menu").menu().hide();
		//Enable dialog widget. More info here: https://jqueryui.com/dialog/
		dialog = $("#dialog").dialog({
			autoOpen: false,
			height: 400,
			width: 400,
			modal: true,
			close: function () {
				dialogOwner = null;
			},
        });
        //Dialog for document types (buttons will be updated when opened)
        dialogDocType = $("#dialog-form-doc-type").dialog({
            autoOpen: false,
            height: 400,
            width: 350,
            modal: true,
            close: function () {
                form[0].reset();
                formfields.removeClass("ui-state-error");
            }
        });
        //Dialog for index fields (buttons will be updated when opened)
        dialogField = $("#dialog-form-field").dialog({
            autoOpen: false,
            height: 400,
            width: 350,
            modal: true,
            close: function () {
                form[0].reset();
                formfields.removeClass("ui-state-error");
            }
        });
        //Dialog for tables (buttons will be updated when opened)
        dialogTable = $("#dialog-form-table").dialog({
            autoOpen: false,
            height: 400,
            width: 350,
            modal: true,
            close: function () {
                form[0].reset();
                formfields.removeClass("ui-state-error");
            }
        });
        //Dialog for columns (buttons will be updated when opened)
        dialogColumn = $("#dialog-form-column").dialog({
            autoOpen: false,
            height: 400,
            width: 350,
            modal: true,
            close: function () {
                form[0].reset();
                formfields.removeClass("ui-state-error");
            }
        });

        jsonDialog = $("#dialogJson").dialog({
            autoOpen: false,
            height: 400,
            width: 400,
            modal: true,
        });

        //context menu for platform provided data. More info on : https://swisnl.github.io/jQuery-contextMenu/
        $.contextMenu({
            selector: '.context-menu-one',
            trigger: 'left',
            build: function ($triggerElement, e) {
                jsonDialog.dialog("option", "buttons", [
                    {
                      text:  $.localize.data.activity.jsonDialog.cancel,
                      click: function () {
                        jsonDialog.dialog("close");
                      }
                    }
                ]);
                return {
                    callback: function (key, options) {
                        if (key == "workitem") {
                            jsonEditorDialog.set(bridge.GetActivityInstance());
                            jsonDialog.dialog("option", "title", $.localize.data.activity.jsonDialog.titleInstance);
                            jsonDialog.dialog("open");
                        } else if (key.indexOf("mt-") == 0) {
                            var name = key.replace("mt-", "");
                            jsonEditorDialog.set(bridge.GetMediaTypeByName(name));
                            jsonDialog.dialog("option", "title", $.localize.data.activity.jsonDialog.titleMedia);
                            jsonDialog.dialog("open");
                        } else if (key.indexOf("dt-") == 0) {
                            var name = key.replace("dt-", "");
                            jsonEditorDialog.set(bridge.GetDocumentTypeByName(name));
                            jsonDialog.dialog("option", "title", $.localize.data.activity.jsonDialog.titleDocType);
                            jsonDialog.dialog("open");
                        } else if (key == "versions") {
                            talkToPlatform();
                        }
                        var m = "clicked: " + key;
                        console.log(m) ;
                    },
                    items: {
                        "workitem": { name: $.localize.data.activity.contextMenus.workItems },
                        "documents": { name: $.localize.data.activity.contextMenus.docTypes, items: loadDocumentTypes() },
                        "medias": { name: $.localize.data.activity.contextMenus.mediaTypes, items: loadMediaTypes() },
                        "versions": {name: $.localize.data.activity.contextMenus.versions}
                    }
                }
            }
        });

		//Enable variable assignment/remove buttons. More info here:https://jqueryui.com/button/
		$(".remVar").button({
			icon: "ui-icon-circle-close",
			showLabel: false
		}).on("click", clearVariable);
	   
		$(".addVar").button({
			icon: "ui-icon-circle-plus",
			showLabel: false
		}).on("click", fillVariablesAndOpenDialog);
		$(".addDbBtn").button();
        $(".addDocTypeBtn").button();
        //enable button for adding a list item
        $(".addListItemBtn").button().on("click", addNewListItem);

		$(".remVar").hide();
		$(":disabled").hide();

		//Enable JSON editor. More info here: https://github.com/josdejong/jsoneditor
		var container = $('#jsoneditor');
		var options = {};
        jsonEditor = new JSONEditor(container[0], options);

        var containerDialog = $('#jsoneditorDialog');
        var options = {};
        jsonEditorDialog = new JSONEditor(containerDialog[0], options);

		//Enable communication between platform and activity. 
		//The first callback in function is void passing a single parameter with activity configuration. This one is called when activity UI is fully loaded and platform passes the activity configuration.
		//The second callback is a function must return the modified activity configuration. This is the configuration platform will store. It is called by the platform when user presses OK on the dialog.
		bridge = new STG.API.PlatformBridge(LoadedDataCallback, SaveDataCallback);

	});
});

//Performs localization of the given object to the given language. 
///If object is not given it assumes entire website,
/// if language is not given it assumes english. 
function doLocalization(lang, obj) {
    if(!lang || lang.length === 0){
        lang = "en";
    }
    if (obj) {
        obj.find("[data-localize]").localize("activity", { language: lang, pathPrefix: "./Resources/i18n" });
    } else {
        $("[data-localize]").localize("activity", { language: lang, pathPrefix: "./Resources/i18n" });
    }
    
}

//Performs a http request to platform using data provided by bridge
function talkToPlatform() {
    if(bridge.GetPlatformVersion() != "unknown"){
        var ai = bridge.GetActivityInstance();
        var baseUrl = bridge.GetDesigntimeServiceUrl();
        $.ajax({
            url: baseUrl + '/processversions/' + ai.Process.ProcessType,
            type: "GET",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            headers: {
                'authorization': 'Bearer ' + bridge.GetAuthToken()
            },
            success: function (result) {
                console.log(result);
                const template = $.localize.data.activity.alerts.processVersions;
                var m = template.replace("{0}", ai.Process.Name);
                m = m.replace("{1}", result.length);
                alert(m);
             },
             error: function (error) {
                console.log(result);
                alert($.localize.data.activity.alerts.uncaughtError )
             }
        });
    } else {
        alert($.localize.data.activity.alerts.featureNotSupported)
    }
}
//loads the platform provided document types as sub-items
function loadDocumentTypes() {
    var subitems = {};

    var dt = bridge.GetAvailableDocumentTypes();

    for (var i = 0; i < dt.length; i++) {
        subitems['dt-' + dt[i]] = { name: dt[i]};
    }
    
    return subitems;
};

//loads the platform provided media types as sub-items
function loadMediaTypes() {
    var subitems = {};

    var mt = bridge.GetAvailableMediaTypes();

    for (var i = 0; i < mt.length; i++) {
        subitems['mt-' + mt[i]] = { name: mt[i]};
    }

    return subitems;
};

//Assigns the variable to the field when user closes the dialog
function assignVariable() {
	//get the checked variable
	var checked = $("#radioset :radio:checked");

	if (checked.length > 0) {

		//get the name of the checked variable
		var varName = checked.attr("varName");
		//get the value of the checked variable
		var varValue = checked.val();

		//get the variable created controls
		var addBtn = $(dialogOwner) //Button that opened dialog
		var ctrl = addBtn.siblings("#" + dialogOwner.id.replace('VarBtn', '')); //control showing the configuration field
		var variableCtrl = addBtn.siblings("#" + dialogOwner.id.replace('VarBtn', 'Var')); //control showing the variable value field
		var remBtn = addBtn.siblings("#" + dialogOwner.id.replace('VarBtn', 'VarRemBtn')); //Button for removing variables

		//get the value how variable should be stored in platform (full path to the property name e.g. Grandfather.father.child)
		var variablePath = getVariablePathForControl(ctrl);
		if (variablePath.length > 0) {
			//assign the variable to the property. Call the AssignVariableByName method on the API script passing in the full path of the property and the name of the variable
			if (bridge.AssignVariableByName(variablePath, varName)) {
				//fix UI of the variable and remember original value; It behaves differently for check-box and other inputs 
				if (ctrl.is(':checkbox')) {
					ctrl.attr("orig", ctrl.prop("checked"));
					ctrl.prop("checked", varValue.toLowerCase() == "true");
				} else {
					ctrl.attr("orig", ctrl.val());
					ctrl.val(varValue);
				}
				//Hide the property control
				ctrl.hide();
				//add variable value to variable input and show it
				variableCtrl.val("$" + varName + "$");
				variableCtrl.show();
				//show button for removing variable
				remBtn.show();
				//hide button for assigning variable
				addBtn.hide();
			}
		}
		//close the dialog
		dialog.dialog("close");
		return true;
	} else {
		//if no variable is selected and OK is pressed ask user to assign variable
		alert($.localize.data.activity.alerts.assign);
		return false;
	}
}

//Based on the selected control this method returns the variable key. The variable key is a string representation of the property full path in JSON.
//Imagine the JSON like this: {"PropA0":{"PropA1": {"PropA2":"ValA2"}, "PropB1":"ValB1"}, "PropB0":[{"PropD0":"ValD0"},{"PropD1":"ValD1"}], "PropC0":"ValC0"}
//Assigning variable  to property PropC0 should return "PropC0"
//Assigning variable  to property PropB1 should return "PropA0.PropB1"
//Assigning variable  to property PropA2 should return "PropA0.PropA1.PropA2"
//Assigning variable  to property PropD1 should return "PropB0[1].PropD1"
function getVariablePathForControl(control) {
	//Based on the selected field that can accept variables, return the full variable path
	switch (control.attr('id')) {
		case "chbCheckBox":
			return "CheckBoxData";
		case "numCBDependent":
            return "CBDependentItem";
        case "listItmCBDependent":
            var level = getRowLevel(control);
            return "LstDependent[" + level + "]"; 
		case "txtServerName":
			var level = getDBLevel(control);
			if (level <= 0) {
				return "";
			} else if (level == 1) {
				//If first control this is not from array; return the variable path
				return "DatabaseLookup.DatabaseConnection.ServerName"
			} else {
				//All others are from array, construct path based on array position
				return "DatabaseLookups[" + (level - 2) + "].DatabaseConnection.ServerName"
			}
		case "txtDatabaseName":
			var level = getDBLevel(control);
			if (level <= 0) {
				return "";
			} else if (level == 1) {
				return "DatabaseLookup.DatabaseConnection.DatabaseName"
			} else {
				return "DatabaseLookups[" + (level - 2) + "].DatabaseConnection.DatabaseName"
			}
		case "txtUsername":
			var level = getDBLevel(control);
			if (level <= 0) {
				return "";
			} else if (level == 1) {
				return "DatabaseLookup.DatabaseConnection.Username"
			} else {
				return "DatabaseLookups[" + (level - 2) + "].DatabaseConnection.Username"
			}
		case "txtPassword":
			var level = getDBLevel(control);
			if (level <= 0) {
				return "";
			} else if (level == 1) {
				return "DatabaseLookup.DatabaseConnection.Password"
			} else {
				return "DatabaseLookups[" + (level - 2) + "].DatabaseConnection.Password"
			}
		default:
			return "";
	}

}

//for list items figure out which object in array the control is
function getRowLevel(control) {
    var trOwner = control.closest('tr');
    if (trOwner.length == 0) {
		//if there is no one, this is not a list item
		return -1;
	} else {
		//Find the ID of control (e.g. lstItmRw3, and get the numeric part only. Note the + as the first item that converts it to number.
		return +trOwner.attr('id').replace("lstItmRw", "");
	}
}

//For the database lookup configurations, figure out which object in array the control is
function getDBLevel(control) {
	//get the div containing the DB lookup UI
	var divOwner = control.closest(".dbLookup");
	if (divOwner.length == 0) {
		//if there is no one, this is not a DB lookup control
		return -1;
	} else {
		//Find the ID of control (e.g. db3, and get the numeric part only. Note the + as the first item that converts it to number.
		return +divOwner.attr('id').replace("db", "");
	}
}

//Event handler for click on Remove variable button. 
//Clears the variable form the field 
function clearVariable(e) {
	//get variable controls
	var ownerID = e.currentTarget.id; 
	var remBtn = $(e.currentTarget);//Remove variable button
	var ctrl = remBtn.siblings("#" + ownerID.replace('VarRemBtn', '')); //Control for the property
	var variableCtrl = remBtn.siblings("#" + ownerID.replace('VarRemBtn', 'Var')); //control for the variable
	var addBtn = remBtn.siblings("#" + ownerID.replace('VarRemBtn', 'VarBtn')); //button for adding the variable

	//Get the variable path
	var variableKey = getVariablePathForControl(ctrl);

	//remove the variable to the property. Call the RemoveVariableAssignement method on the API script passing in the full path of the property
	if (bridge.RemoveVariableAssignement(variableKey)) {
		//get the previous value
		var orig = ctrl.attr("orig");
		//Add previous value to the control and show it
		ctrl.val(orig);
		ctrl.show();
		//empty and hide the control for variable
		variableCtrl.val('');
		variableCtrl.hide();
		//show the add variable button
		addBtn.show();
		//hide the remove variable button
		remBtn.hide();
	}
}

//Event handler for click on Add variable button. 
//fills the available variables for the selected property, and opens the variable assignment dialog
function fillVariablesAndOpenDialog(e) {
	//available variables array
	var availableVariables = [];
	//remember the control that opened the dialog
	dialogOwner = e.currentTarget;
	//Based on the data type of the field, show only variables of acceptable data type, by executing the GetAvailableVariables method on the API script. 
	//Pass one of the following attributes to filter the variables:
	//text = 0, number = 1,date = 3,url = 4,password = 5, checkbox = 7,file = 13
	//To get all available variables, do not pass anything to the method
	switch (e.currentTarget.id) {
		case "chbCheckBoxVarBtn":
			availableVariables = bridge.GetAvailableVariables(7)
			break;
		case "numCBDependentVarBtn":
			availableVariables = bridge.GetAvailableVariables(1)
			break;
		case "txtPasswordVarBtn":
			availableVariables = bridge.GetAvailableVariables(5)
			break;
		default:
			availableVariables = bridge.GetAvailableVariables(0)
			break;
	}

	//clear the "old" variables in the dialog.
	//For displaying the variable list we're using the buttonset widget. More info here:http://api.jqueryui.com/buttonset/
	var varSet = $("#radioset");
	if (varSet.buttonset("instance")) {
		varSet.buttonset("destroy");
	}
	varSet.empty();
	
	if (availableVariables.length > 0) {
		//for each available variable generate a radio button in the buttonset
		for (var i = 0; i < availableVariables.length; i++) {
			var variable = availableVariables[i];
			var option2Add = "<input type='radio' id='var" + i + "' name='var' varName='" + variable.Variable.Name + "'  value='" + variable.Value + "' title='" + variable.Variable.Description + "'><label for='var" + i + "'>" + variable.Variable.Name + "</label>";
			varSet.append(option2Add);
		}
		//hide the info that there is no variables available
		$(".noVariable").hide();
		//show the buttonset widget
		varSet.show();
	} else {
		//if no variables, show the info that there is no variables available 
		$(".noVariable").show();
		//hide the buttonset widget
		varSet.hide();
	}
	//initialize the buttonset widget
	varSet.buttonset();
	//localize and open the dialog
    dialog.dialog( "option", "title",  $.localize.data.activity.varDialog.title );
    dialog.dialog('option', 'buttons', [
        {
            "text": $.localize.data.activity.varDialog.assign,
            "click": assignVariable
        }, {
            "text": $.localize.data.activity.varDialog.cancel,
            "click": function () {
                dialog.dialog("close");
            }
        }
    ]);
	dialog.dialog("open");
}

//Callback function to be passed to platform API script. Platform bridge calls this function when it receives the activity configuration JSON from platform. The configuration is passed as parameter.
function LoadedDataCallback(cfg) {
    const uiLang = getCurrentUILanguage()//fetch the UI language
    if(uiLang !== 'en'){ //apply the language if exists and not english
        doLocalization(uiLang);
    }
	//store the activity configuration JSON
    activitySettings = cfg;
    //adjust platform provided data in context menus

	//initialize UI based on the configuration JSON
	InitUI();
}

//initialize the UI based on the values from JSON
function InitUI() {
	//set up the check-box
	//check/un-check it based on the property value
	$("#chbCheckBox").prop("checked", activitySettings.CheckBoxData);
	//check if the property has variable assigned.
	var variable = bridge.GetAssignedVariable("CheckBoxData");
	if (variable) {
		//if there is a variable assigned adjust UI to display variable assignment
		adjustControlsForVariable("#chbCheckBox", variable, true);
		//enable/disable the dependent input based on variable value
		if (variable.Value.toLowerCase() == "true") {
            $('#numCBDependent').prop("disabled", false);
            $('#listCBDependentAddBtn').prop("disabled", false);
		} else {
            $('#numCBDependent').prop("disabled", true);
            $('#listCBDependentAddBtn').prop("disabled", true);
		}
	} else {
		//No variable; enable/disable the dependent numeric input based on the check-box status
		if (activitySettings.CheckBoxData) {
            $('#numCBDependent').prop("disabled", false);
            $('#listCBDependentAddBtn').prop("disabled", false);
		} else {
            $('#numCBDependent').prop("disabled", true);
            $('#listCBDependentAddBtn').prop("disabled", true);
		}
	}
    
	//assign value to the numeric input
	$('#numCBDependent').val(activitySettings.CBDependentItem);
	//check if the property has variable assigned.
	variable = bridge.GetAssignedVariable("CBDependentItem");
	if (variable) {
		//if there is a variable assigned adjust UI to display variable assignment
		adjustControlsForVariable("#numCBDependent", variable, false);
    }

    //Fills the list property
    if(activitySettings.LstDependent && activitySettings.LstDependent.length > 0){
        activitySettings.LstDependent.forEach(itm=> {
            var order = 0;
            var tr = $('<tr></tr>');
            tr.attr('id', 'lstItmRw' + order);
            $('#listTblBody').append(tr);
            tr.load("RowItemTemplate.html", function () {
                rowTemplateLoaded(order, itm, tr);
            });
        })
    }

    //initialise the platform data tab
    initPlatformData();

	//initialize the JSON editor
	initJson();

	//generate DB lookups
	initDB();
}
//triggered when add new item button is pressed
function addNewListItem(){
    var order = $('#listTblBody tr').length;
    var tr = $('<tr></tr>');
    tr.attr('id', 'lstItmRw' + order);
    $('#listTblBody').append(tr);
    tr.load("RowItemTemplate.html", function () {
        rowTemplateLoaded(order, "", tr);
    });
}

//triggered when row template has been loaded
function rowTemplateLoaded(index, value, tr){
    doLocalization(getCurrentUILanguage(), tr);
    //initialize remove variable buttons
	tr.find(".remVar").button({
		icon: "ui-icon-circle-close",
		showLabel: false
	}).on("click", clearVariable);
	//initialize add variable buttons
	tr.find(".addVar").button({
		icon: "ui-icon-circle-plus",
		showLabel: false
	}).on("click", fillVariablesAndOpenDialog);
    //find if variable is assigned to item
    var variable = bridge.GetAssignedVariable("LstDependent[" + index + "]");
    
    if(! variable){
        //no variable: 
        //hide all variable values controls
	    tr.find(":disabled").hide();
	    //hide all remove variable buttons
        tr.find(".remVar").hide();
        //assign value
        tr.find('#listItmCBDependent').val(value);
    } else {
        //adjust to show variable usage
        adjustControlsForVariable("#listItmCBDependent", variable, false);
    }
}

//fills the platform related data
function initPlatformData() {
    initMediaData();

    initDocTypeData(activitySettings.DocumentType, false);
    initDocTypeData(activitySettings.DocumentType, true);
}

//fills the media type related data
function initMediaData() {
    //find the dropdown for the media types
    var mediaDD = $('#selMediaTypes')
    //iterate trough the media types and append items in the dropdown list
    $.each(bridge.GetAvailableMediaTypes(), function () {
        mediaDD.append($("<option />").val(this).text(this));
    });
    //ensure that the media type stored in the configuration gets selected
    mediaDD.val(activitySettings.MediaType);
}

//fills the document type related data
function initDocTypeData(docTypeName, isEditor) {
    var docTypeID = isEditor ? '#selDocTypesEditor' : '#selDocTypes';
    //find the dropdown for the document types
    var docDD = $(docTypeID);
    //clear drop down
    docDD.empty();
    //iterate trough the document types and append items in the dropdown list
    $.each(bridge.GetAvailableDocumentTypes(), function () {
        docDD.append($("<option />").val(this).text(this));
    });
    //ensure that the document type stored in the configuration gets selected
    docDD.val(docTypeName);

    //fill the fields
    fillFieldsForDocType(docTypeName, isEditor);

    if (isEditor) {
        //fill the tables
        fillTablesForDocType(docTypeName);
    }
}

//fills the fields UI based on the provided document type
function fillFieldsForDocType(docType, isEditor) {
    var fieldSetID = isEditor ? "#fieldSetEditor" : "#fieldSet";
    var noFieldsID = isEditor ? ".noFieldsEditor" : ".noFields";

    //clear the "old" index fields in the dialog.
    //For displaying the field list we're using the buttonset widget. More info here:http://api.jqueryui.com/buttonset/
    var fldSet = $(fieldSetID);
    if (fldSet.buttonset("instance")) {
        fldSet.buttonset("destroy");
    }
    fldSet.empty();

    if (docType && docType.length > 0) {
        //for each field generate a checkbox or a button in the buttonset
        var fieldList = bridge.GetAvailableFieldsForDocumentType(docType);
        for (var i = 0; i < fieldList.length; i++) {
            var field = fieldList[i];
            var option2Add = getFieldElement(docType, field, i, isEditor);
            fldSet.append(option2Add);
        }

        if (isEditor) {
            //add + button to the end of the list
            var addFieldOption = getAddElement("openAddFieldDialog");
            fldSet.append(addFieldOption);
        }

        //hide the info that there is no fields available
        $(noFieldsID).hide();
        //show the buttonset widget
        fldSet.show();
    } else {
        //if no fields, show the info that there is no fields available 
        $(noFieldsID).show();
        //hide the buttonset widget
        fldSet.hide();
        return;
    }

    //initialize the buttonset widget
    fldSet.buttonset();

    // check selected index fields if not in editor
    if (!isEditor) {
        //get the checkbox items
        var checkBoxes = $(fieldSetID + " input");
        checkBoxes.each(function () {
            if (activitySettings && activitySettings.IndexFields && activitySettings.IndexFields.indexOf($(this).val()) != -1) {
                $(this).prop('checked', true);
            }
        });
    }

    fldSet.buttonset("refresh");
}

//gets the HTML of the index field input element
function getFieldElement(docType, field, index, isEditor) {
    var fldID = (isEditor ? "fldEdt" : "fld") + index;
    var option2Add = isEditor ?
        "<input type='button' id='" + fldID + "' name='" + fldID + "' value='" + field.Name + "' title='" + field.Name + "' onclick='openEditFieldDialog(event, " + JSON.stringify(field) + ")'><input type='button' onclick='deleteField(event, &#39;" + docType + "&#39;, &#39;" + field.Name + "&#39;)' value='&#10006;'>" :  // &#9760;

        "<input type='checkbox' id='" + fldID + "' name='" + fldID + "' value='" + field.Name + "' title='" + field.Name + "'><label for='" + fldID + "'>" + field.Name + "</label>";
    return option2Add;
}

//fills the tables UI based on the provided document type
function fillTablesForDocType(docType) {
    //clear the "old" tables in the dialog.
    //For displaying the table list we're using the buttonset widget. More info here:http://api.jqueryui.com/buttonset/
    var tblSet = $("#tableSet");
    if (tblSet.buttonset("instance")) {
        tblSet.buttonset("destroy");
    }
    tblSet.empty();

    // celar columns
    fillColumnsForTable(null, null);

    if (docType && docType.length > 0) {
        //for each table generate a radio button in the buttonset
        var tableList = bridge.GetAvailableTablesForDocumentType(docType);
        for (var i = 0; i < tableList.length; i++) {
            var table = tableList[i];
            var option2Add = "<input type='radio' id='tbl" + i + "' name='table' value='" + table.Name + "' title='" + table.Name + "' onchange='tableChange(event, &#39;" + table.Name + "&#39;)'><label for='tbl" + i + "'>" + table.Name + "</label><input type='button' onclick='openEditTableDialog(event, " + JSON.stringify(table) + ")' value='&#9998;'><input type='button' onclick='deleteTable(event, &#39;" + docType + "&#39;, &#39;" + table.Name + "&#39;)' value='&#10006'>";
            tblSet.append(option2Add);
        }
        //add + button to the end of the list
        var addTableOption = getAddElement("openAddTableDialog");
        tblSet.append(addTableOption);
        //hide the info that there is no table available
        $(".noTables").hide();
        //show the buttonset widget
        tblSet.show();
    } else {
        //if no tables, show the info that there is no tables available 
        $(".noTables").show();
        //hide the buttonset widget
        tblSet.hide();
        return;
    }

    //initialize the buttonset widget
    tblSet.buttonset();
}

//fills the columns UI based on the provided table
function fillColumnsForTable(docType, table) {
    //clear the "old" columns in the dialog.
    //For displaying the columns list we're using the buttonset widget. More info here:http://api.jqueryui.com/buttonset/
    var colSet = $("#columnSet");
    if (colSet.buttonset("instance")) {
        colSet.buttonset("destroy");
    }
    colSet.empty();

    if (table && table.length > 0) {
        //for each column generate a checkbox button in the buttonset
        var columnList = bridge.GetAvailableColumnsForTable(docType, table);
        for (var i = 0; i < columnList.length; i++) {
            var column = columnList[i];
            var option2Add = "<input type='button' id='col" + i + "' name='col" + i + "' value='" + column.Name + "' title='" + column.Name + "' onclick='openEditColumnDialog(event, " + JSON.stringify(column) + ")'><input type='button' onclick='deleteColumn(event, &#39;" + docType + "&#39;, &#39;" + table + "&#39;, &#39;" + column.Name + "&#39;)' value='&#10006;'>"; // &#9760;
            colSet.append(option2Add);
        }
        //add + button to the end of the list
        var addColumnOption = getAddElement("openAddColumnDialog");
        colSet.append(addColumnOption);
        //show the buttonset widget
        colSet.show();
    } else {
        //hide the buttonset widget
        colSet.hide();
        return;
    }

    //initialize the buttonset widget
    colSet.buttonset();
}

//if a field has variable assigned hide field and add variable button, and show variable name and remove variable button
function adjustControlsForVariable(selector, variable, isCheckbox) {
	//get the current value and store it on the "orig" attribute of the control
	if (isCheckbox) {
		$(selector).attr("orig", $(selector).prop("checked"));
		$(selector).prop("checked", variable.Value.toLowerCase() == "true");
	} else {
		$(selector).attr("orig", $(selector).val());
		$(selector).val(variable.Value);
	}
	//set the variable name to the variable input and  show it
	$(selector + "Var").val("$" + variable.Variable.Name + "$");
	$(selector + "Var").show();
	//hide the control that shows the field value
	$(selector).hide();
	//hide the add variable button
	$(selector + "VarBtn").hide();
	//show the remove variable button
	$(selector + "VarRemBtn").show();
}

//initialize the JSON editor
function initJson() {
	//if there is a binary file uploaded to the settings
	if (activitySettings.JSONSettings) {
		//get the contents
		var data = activitySettings.JSONSettings.Content;
		if (data) {
			//split and get the base64 serialized content of the JSON file
			var encoded = data.substring(data.indexOf(',') + 1);
			//decode the contents
			var jsonStr = window.atob(encoded);
			//pass the JSON to editor
			jsonEditor.set(JSON.parse(jsonStr));
			//show the filename
			$('#txtUploadedFile').val(activitySettings.JSONSettings.Filename);
		}
	}
}

//initialize the UI for configured DB lookups
function initDB() {
	//the first one is always present; so add it
	addDBLookup(1);

	//iterate trough array and add others
	for (var i = 0; i < activitySettings.DatabaseLookups.length; i++) {
		addDBLookup(i + 2);
	}
}

//Generates the DB configuration UI
function addDBLookup(order) {
	//create empty DIV
	var div = document.createElement('div');
	//add class and ID to be able to access it later
	$(div).addClass("dbLookup");
	$(div).attr('id', 'db' + order);
	//append it to container
	$(div).appendTo($(".divDatabaseLookupsContainer"))
	//load the contents of the newly created DIV from DBLookupTemplate.html
	var t = $(div).load("DBLookupTemplate.html", function () {
	    templateLoaded(order, div);
	});
}

//callback function executed when JQUERY loads the external html. Accepts newly created DIV element
function templateLoaded(order, div) {
    doLocalization(getCurrentUILanguage(), $(div));
	//initialize remove variable buttons
	$(div).find(".remVar").button({
		icon: "ui-icon-circle-close",
		showLabel: false
	}).on("click", clearVariable);
	//initialize add variable buttons
	$(div).find(".addVar").button({
		icon: "ui-icon-circle-plus",
		showLabel: false
	}).on("click", fillVariablesAndOpenDialog);
	//initialize remove DB config button
	$(div).find(".a-button").button({
		icon: "ui-icon-closethick",
		showLabel: false
	});

	//hide all variable values controls
	$(div).find(":disabled").hide();
	//hide all remove variable buttons
	$(div).find(".remVar").hide();

	
	//do not allow removing the first DB configuration item
	if (order == 1) {
		$(div).find("#remBtn").hide();
	}

	//if we have the activity settings, fill the generated UI with data
	if (activitySettings) {
		if (order == 1) {
			fillDBdata(order, activitySettings.DatabaseLookup);
		} else {
			fillDBdata(order, activitySettings.DatabaseLookups[order - 2]);
		}
	}
	
    //set up the UI for adding controls list to behave as tags input. More info here:https://github.com/xoxco/jQuery-Tags-Input
	$(div).find(".txtColumns").tagsInput();
	$(div).find(".txtFullTextColumns").tagsInput();
}

//Fills the database Lookup UI with platform provided data
function fillDBdata(lookupID, lookupSettings) {
	//find the correct control container
	var item = $(".divDatabaseLookupsContainer").find("#db" + lookupID);

	if (lookupSettings) {
		//set the server name and show variable if assigned
		item.find(".txtServerName").val(lookupSettings.DatabaseConnection.ServerName);
		var variableKey = lookupID == 1 ? "DatabaseLookup.DatabaseConnection.ServerName" : "DatabaseLookups[" + (lookupID - 2) + "].DatabaseConnection.ServerName";
		var variable = bridge.GetAssignedVariable(variableKey);
		if (variable) {
			adjustControlsForVariable("#db" + lookupID + " #txtServerName", variable, false);
		}

		//set the database name and show variable if assigned
		item.find(".txtDatabaseName").val(lookupSettings.DatabaseConnection.DatabaseName);
		variableKey = lookupID == 1 ? "DatabaseLookup.DatabaseConnection.DatabaseName" : "DatabaseLookups[" + (lookupID - 2) + "].DatabaseConnection.DatabaseName";
		variable = bridge.GetAssignedVariable(variableKey);
		if (variable) {
			adjustControlsForVariable("#db" + lookupID + " #txtDatabaseName", variable, false);
		}

		//check the integrated security check box
		item.find(".chbIntegratedSecurity").prop("checked", lookupSettings.DatabaseConnection.IntegratedSecurity);

		//set the user name and show variable if assigned
		item.find(".txtUsername").val(lookupSettings.DatabaseConnection.Username);
		variableKey = lookupID == 1 ? "DatabaseLookup.DatabaseConnection.Username" : "DatabaseLookups[" + (lookupID - 2) + "].DatabaseConnection.Username";
		variable = bridge.GetAssignedVariable(variableKey);
		if (variable) {
			adjustControlsForVariable("#db" + lookupID + " #txtUsername", variable, false);
		}

		//set the password and show variable if assigned
		item.find(".txtPassword").val(lookupSettings.DatabaseConnection.Password);
		variableKey = lookupID == 1 ? "DatabaseLookup.DatabaseConnection.Password" : "DatabaseLookups[" + (lookupID - 2) + "].DatabaseConnection.Password";
		variable = bridge.GetAssignedVariable(variableKey);
		if (variable) {
			adjustControlsForVariable("#db" + lookupID + " #txtPassword", variable, false);
		}
		//hide user-name/password items if integrated security is checked
		if (lookupSettings.DatabaseConnection.IntegratedSecurity) {
			item.find(".divSqlAuthentication").hide();
		} else {
			item.find(".divSqlAuthentication").show();
		}

		//set the DB configuration name
		item.find(".txtName").val(lookupSettings.Name);
		//set the table name
		item.find(".txtTableName").val(lookupSettings.TableName);
		//set the coma separated column names
		item.find(".txtColumns").val(lookupSettings.Columns);

		//check the full text check box
		item.find(".chbFullTextSearch").prop("checked", lookupSettings.FullTextSearch);
		//set the coma separated column names for full text search
		item.find(".txtFullTextColumns").val(lookupSettings.FullTextColumns);
		//set the value for return results number
		item.find(".numMaxResults").val(lookupSettings.MaxResults);

		//show/hide the FullTextColumns depending on the FullTextSearch value
		if (!lookupSettings.FullTextSearch) {
			item.find(".divFullTextColumns").hide();
		} else {
			item.find(".divFullTextColumns").show();
		}
	}
}

//callback function called by API script when user decides to close the dialog. The function must return the activity configuration JSON with updated values
function SaveDataCallback(e) {
    if (e) {
        if (!$('#cb-confirm-data').is(":checked")) {
            e.cancel = true;
            e.reason = $.localize.data.activity.validation;
            return;
        }
    }
	//sync the UI with the activity configuration JSON
	collectFromUI();
	//return updated JSON
	return activitySettings;
}

//get the data from UI and save it on the activity settings
function collectFromUI() {
	//update the check box data
	activitySettings.CheckBoxData = $("#chbCheckBox").prop("checked")
	//update the CBDependentItem
	if (activitySettings.CheckBoxData) {
		activitySettings.CBDependentItem = $('#numCBDependent').val();
	} else {
		activitySettings.CBDependentItem = 0;
    }

    activitySettings.LstDependent = [];
    
    var values = $('#listTblBody #listItmCBDependent')
    values.each(index => {
        activitySettings.LstDependent.push($(values[index]).val())
    });

    //collect the platform data and fill settings
    syncPlatformData();

	//Collect the JSON and update binary settings
	syncJSONData();

	//Collect all configured DB lookups
    syncDBData();
}

//Collect the platform data and fill settings
function syncPlatformData() {
    //get the selected media type
    activitySettings.MediaType = $("#selMediaTypes").val();
    //get the selected document type
    activitySettings.DocumentType = $("#selDocTypes").val();

    if (activitySettings.DocumentType && activitySettings.DocumentType.length > 0) {
        //get the checked items
        var checked = $("#fieldSet input:checked");
        var concat = "";
        checked.each(function () {
            concat += $(this).val() + ";";
        });
        activitySettings.IndexFields = concat;
    } else {
        //no document type selected; return empty
        activitySettings.IndexFields = "";
    }
}

//get the current UI language
function getCurrentUILanguage() {
    let ret = 'en';
    if(bridge && typeof bridge['GetCurrentUILanguage'] === 'function'){ //we have the latest bridge providing the function
        const uiLang = bridge.GetCurrentUILanguage()//fetch the UI language
        if(uiLang && uiLang.length == 2 && uiLang !== 'en'){ //apply the language if exists and not english
            ret = uiLang;
        }
    }
    return ret    
}

//Collect the JSON and update binary settings
function syncJSONData() {
	//check if there is uploaded file or user created JSON manually
	var newFN = $('#txtUploadedFile').val();
	if (newFN == null || newFN.length <= 0) {
		newFN = "manual_edit"
	}

	//get JSON content
	var content = getJsonContet();

	if (content.length > 0) {
		//if there is content update settings with modified data
		if (activitySettings.JSONSettings) {
			activitySettings.JSONSettings.Filename = newFN;
			activitySettings.JSONSettings.ContentType = "application/json";
			activitySettings.JSONSettings.Content = content;
		} else {
			//if there was no content, generate new one
			activitySettings.JSONSettings = {
				Filename: newFN,
				Size: size,
				ContentType: "application/json",
				Content: content
			};
		}
	}
}

//get JSON content encoded as base64 string
function getJsonContet() {
	//get teh JSON from editor
	var json = jsonEditor.get();
	//stringify it
	var jsonString = JSON.stringify(json, null, 2);
	if (jsonString.length > 0) {
		//convert it to base64 string and return
		var encoded = window.btoa(jsonString);
		return "data:;base64," + encoded
	} else {
		//no JSON, return empty string
		return "";
	}
}

//collects the DB lookup data from UI and sync it with JSON settings
function syncDBData() {
	//find all the configured lookups
	var dbs = $(".divDatabaseLookupsContainer").find(".dbLookup");
	//clear the lookup array
	activitySettings.DatabaseLookups = [];
	if (dbs.length == 0) {
		//no lookups; return an empty first one
		activitySettings.DatabaseLookup = createEmptyLookupObject();
	} else {
		//iterate trough configured lookups and generate collect data
		for (var i = 0; i < dbs.length; i++) {
			var dataJQ = $(dbs[i]);
			if (i == 0) {
				//for the first create object
				activitySettings.DatabaseLookup = createAndFillLookupObject(dataJQ);

			} else {
				//for others add in array
				activitySettings.DatabaseLookups.push(createAndFillLookupObject(dataJQ));
			}
		}
	}

}

//generates empty (default) lookup object
function createEmptyLookupObject() {
	return {
		DatabaseConnection: {
			ServerName: "[sql server name]",
			DatabaseName: "[database name]",
			IntegratedSecurity: false,
			Username: "[user name]",
			Password: "[password]"
		},
		Name: "",
		TableName: "",
		Columns: "",
		FullTextSearch: false,
		FullTextColumns: "",
		MaxResults: 100
	};
}

//Generates lookup object and fills it with data from UI
function createAndFillLookupObject(dbData) {
	var ret = {
		DatabaseConnection: {
			ServerName: dbData.find(".txtServerName").val(),
			DatabaseName: dbData.find(".txtDatabaseName").val(),
			IntegratedSecurity: dbData.find(".chbIntegratedSecurity").prop("checked"),
			Username: dbData.find(".txtUsername").val(),
			Password: dbData.find(".txtPassword").val()
		},
		Name: dbData.find(".txtName").val(),
		TableName: dbData.find(".txtTableName").val(),
		Columns: dbData.find(".txtColumns").val(),
		FullTextSearch: dbData.find(".chbFullTextSearch").prop("checked"),
		FullTextColumns: dbData.find(".txtFullTextColumns").val(),
		MaxResults: dbData.find(".numMaxResults").val()
	};

	return ret;
}

//Update the existing lookup object with values from UI
function updateLookup(dbConnectionObject, dbData) {
	dbConnectionObject.DatabaseConnection.ServerName = dbData.find(".txtServerName").val(),
	dbConnectionObject.DatabaseConnection.DatabaseName = dbData.find(".txtDatabaseName").val(),
	dbConnectionObject.DatabaseConnection.IntegratedSecurity = dbData.find(".chbIntegratedSecurity").prop("checked"),
	dbConnectionObject.DatabaseConnection.Username = dbData.find(".txtUsername").val(),
	dbConnectionObject.DatabaseConnection.Password = dbData.find(".txtPassword").val()

	dbConnectionObject.Name = dbData.find(".txtName").val(),
	dbConnectionObject.TableName = dbData.find(".txtTableName").val(),
	dbConnectionObject.Columns = dbData.find(".txtColumns").val(),
	dbConnectionObject.FullTextSearch = dbData.find(".chbFullTextSearch").prop("checked"),
	dbConnectionObject.FullTextColumns = dbData.find(".txtFullTextColumns").val(),
	dbConnectionObject.MaxResults = dbData.find(".numMaxResults").val()
}

//Change event handler for the check box. 
//When check box gets checked the dependent item gets enabled and vice-versa
function cbChange(evt) {
	if (evt.currentTarget.name == 'CheckBox') {
        var dependent = $('#numCBDependent');
        var dependantBtn = $('#listCBDependentAddBtn');
		if (evt.currentTarget.checked) {
            dependent.prop("disabled", false);
            dependantBtn.prop("disabled", false);
		} else {
            dependent.prop("disabled", true);
            dependantBtn.prop("disabled", true);
		}
	}
}

//Event handler for click on upload file input
//clear the target on click
function fileClick(evt) {
	evt.currentTarget.value = null;
}

//Event handler for change of the document type class
function docTypeChange(event) {
    onDocTypeChange(false);
}

//Event handler for change of the document type class in editor
function docTypeChangeEditor(event) {
    onDocTypeChange(true);
}

function onDocTypeChange(isEditor) {
    var docTypeID = isEditor ? '#selDocTypesEditor' : '#selDocTypes';
    var docTypeName = $(docTypeID).val();

    fillFieldsForDocType(docTypeName, isEditor);

    if (isEditor) {
        fillTablesForDocType(docTypeName);
    }
}

//Event handler for change of the document type class
function tableChange(event, tableName) {
    var docDD = $('#selDocTypesEditor');
    
    fillColumnsForTable(docDD.val(), tableName);
}

//Event handler for change on upload file input
function fileChange(evt) {
	//get uploaded file
	file = evt.currentTarget.files[0];
	$('#txtUploadedFile').val(file.name);
	size = file.size;
	//if uploaded, read the file and set up content in JSON editor
	if (file) {
		fr = new FileReader();
		fr.onload = function () {
			jsonEditor.set(JSON.parse(fr.result));
		}
		fr.readAsText(file);
	}
}

//Change event handler for the check box. 
//When check box gets checked the columns gets shown/hidden
function toggleDiv(sender, divID, showIfchecked) {
	$(sender).closest('fieldset').find(divID).toggle(sender.checked == showIfchecked);
}

//Click event handler for the add database lookup button. 
function addDatabaseLookup() {
	//get number of configured lookups
	var num = $(".divDatabaseLookupsContainer").find(".dbLookup").length;
	//add new one
	addDBLookup(num + 1);
}

//Click event handler for removing the DB lookup. 
function removeDatabaseLookup(sender) {
	$(sender).closest('.dbLookup').remove();
}

//Get the add + button
function getAddElement(callbackFunctionName) {
    return "<input type='button' onclick='" + callbackFunctionName + "(event)' value='&#10010;'>";     // &#9773;
}

//Event handler for opening the add document type dialog
function openAddDocTypeDialog() {
    //Get all input fields in the form
    formfields = $([]).add($("#docTypeName")).add($("#docTypeName"));
    //Remove error class
    formfields.removeClass("ui-state-error");

    //Update OK button function call
    dialogDocType.dialog('option', 'buttons', [
        {
            "text": $.localize.data.activity.docTypeDialog.ok,
            "click": addDocType
        },{
            "text": $.localize.data.activity.docTypeDialog.cancel,
            "click": function () {
                dialogDocType.dialog("close");
            }
        }
    ]);
    //Add submit event to form
    form = dialogDocType.find("form").on("submit", function (event) {
        event.preventDefault();
        addDocType();
    });
    //localize and Show the dialog
    dialogDocType.dialog("option", "title",  $.localize.data.activity.docTypeDialog.title );
    dialogDocType.dialog("open");
}

//Add a new empty document type
function addDocType() {
    //Validate form fields
    var valid = validateDocType();
    if (valid) {
        //Try to add the document type
        var isAdded = bridge.AddDocumentType($("#docTypeName").val(), $("#docTypeDescription").val());
        if (isAdded) {
            //Refresh UI
            initDocTypeData($("#docTypeName").val(), true);
            //Close the dialog
            dialogDocType.dialog("close");
        } else {
            //Duplicate document type name detected
            alert($.localize.data.activity.alerts.docTypeWithNameExists);
        }
    }

    return valid;
}

//Validate a document type
function validateDocType() {
    var isValid = true;
    formfields.removeClass("ui-state-error");

    var docTypeName = $("#docTypeName");

    //Check if fields are valid
    isValid = isValid && checkIfEmpty(docTypeName, "name");

    return isValid;
}

// INDEX FIELDS CRUD ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Event handler for opening the add field dialog
function openAddFieldDialog(event) {
    //Get all input fields in the form
    formfields = $([]).add($("#fieldName")).add($("#fieldType")).add($("#isIndex"));
    //Remove error class
    formfields.removeClass("ui-state-error");

    //Update OK button function call
    dialogField.dialog('option', 'buttons', [{
        text: $.localize.data.activity.fieldDialog.ok,
        click: addField
    }, {
        text: $.localize.data.activity.fieldDialog.cancel,
        click: function () {
            dialogField.dialog("close");
        }
    }
    ]);
    dialogField.dialog( "option", "title",  $.localize.data.activity.fieldDialog.title );
    //Add submit event to form
    form = dialogField.find("form").on("submit", function (event) {
        event.preventDefault();
        addField();
    });
    //Show the dialog
    dialogField.dialog("open");
}

//Event handler for opening the edit field dialog
function openEditFieldDialog(event, field) {
    //Get all input fields in the form
    var fieldNameOriginal = $("#fieldNameOriginal");
    var fieldName = $("#fieldName");
    var fieldType = $("#fieldType");
    var fieldIsIndex = $("#fieldIsIndex");
    formfields = $([]).add(fieldName).add(fieldType).add(fieldIsIndex);
    //Remove error class
    formfields.removeClass("ui-state-error");

    //Fill selected index field data
    fieldNameOriginal.val(field.Name);
    fieldName.val(field.Name);
    fieldType.val(field.FieldType);
    fieldIsIndex.prop('checked', field.IsIndexed);

    //Update OK button function call
    //Update OK button function call
    dialogField.dialog('option', 'buttons', [{
        text: $.localize.data.activity.fieldDialog.ok,
        click: updateField
    }, {
        text: $.localize.data.activity.fieldDialog.cancel,
        click: function () {
            dialogField.dialog("close");
        }
    }
    ]);
    dialogField.dialog( "option", "title",  $.localize.data.activity.fieldDialog.title );
    //Add submit event to form
    form = dialogField.find("form").on("submit", function (event) {
        event.preventDefault();
        updateField();
    });
    //Show the dialog
    dialogField.dialog("open");
}

//Add an index field to the document type
function addField() {
    //Validate form fields
    var valid = validateField();
    if (valid) {
        //Get selected document type name
        var docTypeName = $('#selDocTypesEditor').val();
        //Add index field
        bridge.AddField(docTypeName, $("#fieldName").val(), $("#fieldType").val(), $("#fieldIsIndex").is(':checked'));
        //Refresh UI
        fillFieldsForDocType(docTypeName, true);
        //Close the dialog
        dialogField.dialog("close");
    }

    return valid;
}

//Update an index field in the document type
function updateField() {
    //Validate form fields
    var valid = validateField();
    if (valid) {
        //Get selected document type name
        var docTypeName = $('#selDocTypesEditor').val();
        //Update index field
        bridge.UpdateField(docTypeName, $("#fieldNameOriginal").val(), $("#fieldName").val(), $("#fieldType").val(), $("#fieldIsIndex").is(':checked'));
        //Refresh UI
        fillFieldsForDocType(docTypeName, true);
        //Close the dialog
        dialogField.dialog("close");
    }

    return valid;
}

//Validate an index field
function validateField() {
    var isValid = true;
    formfields.removeClass("ui-state-error");

    var fieldName = $("#fieldName");
    var fieldType = $("#fieldType");

    //Check if fields are valid
    isValid = isValid && checkIfEmpty(fieldName, "name");
    isValid = isValid && checkIfEmpty(fieldType, "type");

    return isValid;
}

//Event handler for deleting an index field from the document type
function deleteField(event, docTypeName, fieldName) {
    let msg = $.localize.data.activity.confirms.delFiled;
    msg = msg.replace("{0}", fieldName);
    msg = msg.replace("{1}", docTypeName);
    if (confirm(msg)) {
        //Delete field
        bridge.DeleteField(docTypeName, fieldName);
        //Refresh UI
        fillFieldsForDocType(docTypeName, true);
    }    
}

// TABLES CRUD ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Event handler for opening the add table dialog
function openAddTableDialog(event) {
    //Get all input fields in the form
    formfields = $([]).add($("#tableName")).add($("#tableDescription"));
    //Remove error class
    formfields.removeClass("ui-state-error");

    //Update OK button function call and loclaize
    dialogTable.dialog('option', 'buttons', [{
        text: $.localize.data.activity.tableDialog.ok,
        click: addTable
    }, {
        text: $.localize.data.activity.tableDialog.cancel,
        click: function () {
            dialogTable.dialog("close");
        }
    }
    ]);
    dialogTable.dialog( "option", "title",  $.localize.data.activity.tableDialog.title );
    
    //Add submit event to form
    form = dialogTable.find("form").on("submit", function (event) {
        event.preventDefault();
        addTable();
    });
    //Show the dialog
    dialogTable.dialog("open");
}

//Event handler for opening the edit table dialog
function openEditTableDialog(event, table) {
    //Get all input fields in the form
    var tableNameOriginal = $("#tableNameOriginal");
    var tableName = $("#tableName");
    var tableDescription = $("#tableDescription");
    formfields = $([]).add(tableName).add(tableDescription);
    //Remove error class
    formfields.removeClass("ui-state-error");

    //Fill selected table data
    tableNameOriginal.val(table.Name);
    tableName.val(table.Name);
    tableDescription.val(table.Description);

    //Update OK button function call and localize
    dialogTable.dialog('option', 'buttons', [{
        text: $.localize.data.activity.tableDialog.ok,
        click: updateTable
    }, {
        text: $.localize.data.activity.tableDialog.cancel,
        click: function () {
            dialogTable.dialog("close");
        }
    }
    ]);
    dialogTable.dialog( "option", "title",  $.localize.data.activity.tableDialog.title );
    //Add submit event to form
    form = dialogTable.find("form").on("submit", function (event) {
        event.preventDefault();
        updateTable();
    });
    //Show the dialog
    dialogTable.dialog("open");
}

//Add a table to the document type
function addTable() {
    //Validate form fields
    var valid = validateTable();
    if (valid) {
        //Get selected document type name
        var docTypeName = $('#selDocTypesEditor').val();
        //Add table
        bridge.AddTable(docTypeName, $("#tableName").val(), $("#tableDescription").val());
        //Refresh UI
        fillTablesForDocType(docTypeName);
        //Close the dialog
        dialogTable.dialog("close");
    }

    return valid;
}

//Update a table in the document type
function updateTable() {
    //Validate form fields
    var valid = validateTable();
    if (valid) {
        //Get selected document type name
        var docTypeName = $('#selDocTypesEditor').val();
        //Update table
        bridge.UpdateTable(docTypeName, $("#tableNameOriginal").val(), $("#tableName").val(), $("#tableDescription").val());
        //Refresh UI
        fillTablesForDocType(docTypeName);
        //Close the dialog
        dialogTable.dialog("close");
    }

    return valid;
}

//Validate a table
function validateTable() {
    var isValid = true;
    formfields.removeClass("ui-state-error");

    var tableName = $("#tableName");

    //Check if fields are valid
    isValid = isValid && checkIfEmpty(tableName, "name");

    return isValid;
}

//Event handler for deleting a table from the document type
function deleteTable(event, docTypeName, tabledName) {
    let msg = $.localize.data.activity.confirms.delTable;
    msg = msg.replace("{0}", tabledName);
    msg = msg.replace("{1}", docTypeName); 
    if (confirm(msg)) {
        //Delete table
        bridge.DeleteTable(docTypeName, tabledName);
        //Refresh UI
        fillTablesForDocType(docTypeName);
    }
}

// COLUMNS CRUD ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Event handler for opening the add column dialog
function openAddColumnDialog(event) {
    //Get all input fields in the form
    formfields = $([]).add($("#columnName")).add($("#columnType"));
    //Remove error class
    formfields.removeClass("ui-state-error");

    //Update OK button function call and localize
    dialogColumn.dialog('option', 'buttons', [{
        text: $.localize.data.activity.columnDialog.ok,
        click: addColumn
    }, {
        text: $.localize.data.activity.columnDialog.cancel,
        click: function () {
            dialogColumn.dialog("close");
        }
    }
    ]);
    dialogColumn.dialog( "option", "title",  $.localize.data.activity.columnDialog.title );
    
    //Add submit event to form
    form = dialogColumn.find("form").on("submit", function (event) {
        event.preventDefault();
        addColumn();
    });
    //Show the dialog
    dialogColumn.dialog("open");
}

//Event handler for opening the edit column dialog
function openEditColumnDialog(event, column) {
    //Get all input fields in the form
    var columnNameOriginal = $("#columnNameOriginal");
    var columnName = $("#columnName");
    var columnType = $("#columnType");
    formfields = $([]).add(columnName).add(columnType);
    //Remove error class
    formfields.removeClass("ui-state-error");

    //Fill selected column data
    columnNameOriginal.val(column.Name);
    columnName.val(column.Name);
    columnType.val(column.FieldType);

    //Update OK button function call and localize
    dialogColumn.dialog('option', 'buttons', [{
        text: $.localize.data.activity.columnDialog.ok,
        click: updateColumn
    }, {
        text: $.localize.data.activity.columnDialog.cancel,
        click: function () {
            dialogColumn.dialog("close");
        }
    }
    ]);
    dialogColumn.dialog( "option", "title",  $.localize.data.activity.columnDialog.title );
    //Add submit event to form
    form = dialogColumn.find("form").on("submit", function (event) {
        event.preventDefault();
        updateColumn();
    });
    //Show the dialog
    dialogColumn.dialog("open");
}

//Add a column to the document type
function addColumn() {
    //Validate form fields
    var valid = validateColumn();
    if (valid) {
        //Get selected document type name
        var docTypeName = $('#selDocTypesEditor').val();
        var tableName = $('input[name=table]:checked', '#tableSet').val();
        //Add column
        bridge.AddColumn(docTypeName, tableName, $("#columnName").val(), $("#columnType").val());
        //Refresh UI
        fillColumnsForTable(docTypeName, tableName);
        //Close the dialog
        dialogColumn.dialog("close");
    }

    return valid;
}

//Update a column in the document type
function updateColumn() {
    //Validate form fields
    var valid = validateColumn();
    if (valid) {
        //Get selected document type name
        var docTypeName = $('#selDocTypesEditor').val();
        var tableName = $('input[name=table]:checked', '#tableSet').val();
        //Update column
        bridge.UpdateColumn(docTypeName, tableName, $("#columnNameOriginal").val(), $("#columnName").val(), $("#columnType").val());
        //Refresh UI
        fillColumnsForTable(docTypeName, tableName);
        //Close the dialog
        dialogColumn.dialog("close");
    }

    return valid;
}

//Validate a column
function validateColumn() {
    var isValid = true;
    formfields.removeClass("ui-state-error");

    var columnName = $("#columnName");
    var columnType = $("#columnType");

    //Check if fields are valid
    isValid = isValid && checkIfEmpty(columnName, "name");
    isValid = isValid && checkIfEmpty(columnType, "type");

    return isValid;
}

//Event handler for deleting a column from the document type
function deleteColumn(event, docTypeName, tableName, columnName) {
    let msg = $.localize.data.activity.confirms.delColumn;
    msg = msg.replace('{0}', columnName);
    msg = msg.replace('{1}', tableName);
    msg = msg.replace('{2}', docTypeName);
    if (confirm(msg)) {
        //Delete column
        bridge.DeleteColumn(docTypeName, tableName, columnName);
        //Refresh UI
        fillColumnsForTable(docTypeName, tableName);
    }
}

// checks if the value of an input field is empty
function checkIfEmpty(input, name) {
    if (input.val().length < 1) {
        //Add error class
        input.addClass("ui-state-error");
        //updateTips("Length of " + name + " must not be empty.");
        return false;
    } else {
        return true;
    }
}