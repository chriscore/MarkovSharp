var typingTimer;               //timer identifier
var doneTypingInterval = 200;  //time in ms, 5 second for example
var $trainingArea = $('#training-input');
var $input = $('#testing-input');
var $levelNum = $('#markov-level');

//on keyup, start the countdown
$input.on('keyup', function () {
    if ($input.is(':focus')){
        clearTimeout(typingTimer);
        typingTimer = setTimeout(GetNextPredictions, doneTypingInterval);
    }
    else{
        console.log('not focused');
    }
});

//on keydown, clear the countdown 
$input.on('keydown', function () {
    if ($input.is(':focus')) {
        clearTimeout(typingTimer);
    }
    else {
        console.log('not focused');
    }
});

$("#suggestion-1").on('click', function () {
    insertChoice($("#suggestion-1"));
});

$("#suggestion-2").on('click', function () {
    insertChoice($("#suggestion-2"));
});

$("#suggestion-3").on('click', function () {
    insertChoice($("#suggestion-3"));
});

function insertChoice($element) {
    var currentText = $input.val();
    if ($element.text() !== '-') {
        var newText = currentText.concat(' ', $element.text());
        console.log('Setting new text: '.concat(newText));
        $input.val(newText);

        GetNextPredictions();
    }
}

function Train() {
    var level = $levelNum.val();
    console.log('training model with level ' + level);
    var txt = $trainingArea.val();

    var trainingPostData = {
        modelLevel: level,
        trainingData: txt
    };
    console.log(trainingPostData);

    jQuery.ajax({
        url: "/Home/Train",
        type: "POST",
        data: JSON.stringify(trainingPostData),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data.Message);
            GetNextPredictions();
        }
    });
}

//user is finished typing, get predictions
function GetNextPredictions() {
    console.log('getting predictions');

    var seedText = $input.val();

    var postBody = {
        seedText: seedText
    };
    console.log(postBody);

    jQuery.ajax({
        url: "/Home/GetPredictions",
        type: "POST",
        data: JSON.stringify(postBody),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data);

            $("#suggestion-1").html(data.Suggestions[0] || '-');
            $("#suggestion-2").html(data.Suggestions[1] || '-');
            $("#suggestion-3").html(data.Suggestions[2] || '-');
        }
    });
}