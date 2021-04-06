import React, { CSSProperties } from 'react';
import classNames from 'classnames';

import './spinner.scss';

export interface SpinnerProps {
	children: any,
	text?: string;
    textColor?: string;
	style?: CSSProperties;
	className?: string;
}

export default ({ text, textColor, children, style, className }: SpinnerProps) => {
	const spinnerClassName = classNames('spinner', className);

	return (
		<div className={spinnerClassName} style={{ color: textColor, ...style }}>
			{children}
			{text && <label className={'label'}>{text}</label>}
		</div>
	);
};
