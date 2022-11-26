import {bech32} from "bech32";

export function lnurlPay(username) {
    return lnurlEncode(`https://${window.location.host}/pay/${username}`);
}

export function lnurlWithdraw(id) {
    return lnurlEncode(`https://${window.location.host}/withdraw/${id}`);
}

function lnurlEncode(link) {
    let words = new TextEncoder().encode(link);
    return bech32.encode("lnurl", bech32.toWords(words), 10_000).toUpperCase();
}