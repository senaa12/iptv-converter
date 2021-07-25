import React, { useMemo } from 'react';
import IsMobile from './isMobileCheck';

const mobile = ({ children }: { children: React.ReactNode }) => {
    const isMobile = useMemo(
        IsMobile,
        []
    );

    return (
        <>{isMobile ? <>{children}</> : null}</>
    );
}

export const Desktop = ({ children }: { children: React.ReactNode }) => {
    const isMobile = useMemo(
        IsMobile,
        []
    );

    return (
        <>{!isMobile ? <>{children}</> : null}</>
    );
}

export default mobile;
