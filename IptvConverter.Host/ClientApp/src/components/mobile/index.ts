import IsMobileCheck from './isMobileCheck';
import Mobile, { Desktop } from './mobile';
import classNames from 'classnames';

const IsMobile = IsMobileCheck();

const appendMobileClass = (baseClass: string) => classNames(baseClass, {
    'mobile': !!IsMobile
});


export {
    Mobile as default,
    Desktop, 
    appendMobileClass,
    IsMobileCheck,
    IsMobile
}
