import {bech32} from "bech32";

export function lnurlPay(username, description) {
    let addDesc = description && description.length > 0 ? `?d=${encodeURIComponent(description)}` : "";
    return lnurlEncode(`https://${window.location.host}/p/${username}${addDesc}`);
}

export function lnurlWithdraw(id) {
    return lnurlEncode(`https://${window.location.host}/w/${id}`);
}

export function lnurlWithdrawUri(id) {
    return `lnurlw://${window.location.host}/w/${id}`;
}

function lnurlEncode(link) {
    let words = new TextEncoder().encode(link);
    return bech32.encode("lnurl", bech32.toWords(words), 10_000).toUpperCase();
}