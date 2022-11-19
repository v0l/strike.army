import QRCodeStyling from "qr-code-styling";
import {useEffect, useRef} from "react";

export default function StrikeArmyQR(props) {
    const qrRef = useRef();

    useEffect(() => {
        if (props.link) {
            let qr = new QRCodeStyling({
                width: props.width || 256,
                height: props.height || 256,
                data: props.link,
                margin: 5,
                type: 'canvas',
                image: props.avatar,
                qrOptions: {
                    mode: "Alphanumeric"
                },
                dotsOptions: {
                    type: 'rounded'
                },
                cornersSquareOptions: {
                    type: 'extra-rounded'
                },
                imageOptions: {
                    crossOrigin: "anonymous"
                }
            });
            qrRef.current.innerHTML = "";
            qr.append(qrRef.current);
            qrRef.current.onclick = function (e) {
                let elm = document.createElement("a");
                elm.href = `lightning:${props.link}`;
                elm.click();
            }
        }
    }, [props.link]);

    return (
        <div ref={qrRef}></div>
    );
}