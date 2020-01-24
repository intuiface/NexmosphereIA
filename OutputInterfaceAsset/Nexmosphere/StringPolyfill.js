/**
 * A polyfill is a piece of code used to provide recent functionality on old browsers that do not support them natively
 * Source: http://cwestblog.com/2013/02/26/javascript-string-prototype-matchall/
 */

if (!String.prototype.matchAll) {
    String.prototype.matchAll = function (regexp) {
        const matches = [];
        this.replace(regexp, function () {
            const arr = ([]).slice.call(arguments, 0);
            const extras = arr.splice(-2);
            arr.index = extras[0];
            arr.input = extras[1];
            matches.push(arr);
        });
        return matches.length ? matches : null;
    };
}
