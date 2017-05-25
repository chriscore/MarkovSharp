//setup before functions
var typingTimer;               //timer identifier
var doneTypingInterval = 200;  //time in ms, 5 second for example
var $trainingArea = $('#training-input');
var $input = $('#testing-input');

function Train() {
    console.log('training model');
    var txt = $trainingArea.val();

    $.post("/Home/Train", txt, function (data) {
        console.log(data);
        GetNextPredictions();
    });
}

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

//user is finished typing, get predictions
function GetNextPredictions() {
    console.log('getting predictions');
    var txt = $input.val();

    $.post("/Home/GetPredictions", txt, function (data)
    {
        console.log(data);

        $("#suggestion-1").html(data[0] || '-');
        $("#suggestion-2").html(data[1] || '-');
        $("#suggestion-3").html(data[2] || '-');
    });
}