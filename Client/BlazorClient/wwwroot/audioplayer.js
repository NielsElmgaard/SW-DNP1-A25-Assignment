function unmutePlayer() {
    var audio = document.getElementById('autoPlayAudio');
    if (audio) {
        audio.muted = false; 
        audio.play();
    }
}