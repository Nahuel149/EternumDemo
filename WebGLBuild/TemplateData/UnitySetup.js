function unityShowBanner(msg, type) {
        
    }
    
    var buildUrl = "Build";
    var loaderUrl = buildUrl + "/WebGLBuild.loader.js";
    var config = {
        dataUrl: buildUrl + "/WebGLBuild.data",
        frameworkUrl: buildUrl + "/WebGLBuild.framework.js",
        codeUrl: buildUrl + "/WebGLBuild.wasm",
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
