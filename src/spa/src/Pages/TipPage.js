import "./TipPage.css"
import {useParams} from "react-router-dom";
import StrikeArmyQR from "../Components/StrikeArmyQR";
import {lnurlPay} from "../Util";

export default function TipPage() {
    const route = useParams();
    const username = route.username;

    return (
        <div className="tip-page">
            <div className="tip-widget">
                <h2>Tip {username}!</h2>
                <StrikeArmyQR link={lnurlPay(username)}/>
            </div>
        </div>
    )
}