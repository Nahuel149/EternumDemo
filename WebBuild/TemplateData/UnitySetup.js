function unityShowBanner(msg, type) {
        
    }
    
    var buildUrl = "Build";
    var loaderUrl = buildUrl + "/WebBuild.loader.js";
    var config = {
        dataUrl: buildUrl + "/WebBuild.data",
        frameworkUrl: buildUrl + "/WebBuild.framework.js",
        codeUrl: buildUrl + "/WebBuild.wasm",
        streamingAssetsUrl: "StreamingAssets",
        companyName: "DefaultCompany",
        productName: "Eternum",
        productVersion: "0.1",
        showBanner: unityShowBanner,
    };

    createUnityInstance(canvas, config, (progress) => {
    })
        .then((unityInstance) => {
            gameInstance = unityInstance;
        })
        .catch((message) => {
            alert(message);
        });
