import classNames from 'classnames';
import React, { useEffect } from 'react';
import IconEnum from './iconEnum';

export interface IconProps extends React.SVGProps<SVGSVGElement> {
    iconName: IconEnum;
    testId?: string;
}

const icon = ({ iconName, testId, ...svgProps }: IconProps) => {
    useEffect(() => {
        try {
            require(`../../assets/icons/${iconName}.svg`);
        } catch (ex) {
            console.error(ex);
        }
    }, []);

    const className = classNames(iconName, svgProps.className, 'icon', 'svg-icon', {
        clickable: !!svgProps.onClick,
    });

    return(
        <svg
            {...svgProps}
            className={className}
            data-testid={testId}
            height={svgProps.height ?? 16}
            width={svgProps.width ?? 16}
        >
            <use xlinkHref={`#${iconName}`} />
        </svg>
    );
};

export default icon;
