window.videoInterop = {
    seekTo: function (elementId, seconds) {
        var video = document.getElementById(elementId);
        if (video) {
            video.currentTime = seconds;
            // Play video to show seeked moment if paused, but catch standard browser autoplay policy rejections gracefully
            video.play().catch(function(error) {
                console.warn("Autoplay or programmatic play was blocked/interrupted by browser policy: ", error);
            });
        }
    },
    getCurrentTime: function (elementId) {
        var video = document.getElementById(elementId);
        return video ? video.currentTime : 0;
    },
    triggerPrint: function () {
        window.print();
    }
};
